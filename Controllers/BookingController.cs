using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using _402410745_Staybnb_assignment.Data;
using _402410745_Staybnb_assignment.Models;
using _402410745_Staybnb_assignment.Services;

namespace _402410745_Staybnb_assignment.Controllers;

// Guest-only pages: booking, my bookings, check-in steps and reviews.
[Authorize(Roles = "Guest")]
public class BookingController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _users;
    private readonly AuditService _audit;
    private readonly IWebHostEnvironment _env;

    public BookingController(ApplicationDbContext db, UserManager<ApplicationUser> users,
        AuditService audit, IWebHostEnvironment env)
    {
        _db = db;
        _users = users;
        _audit = audit;
        _env = env;
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int propertyId, DateTime checkIn, DateTime checkOut, int guests)
    {
        var property = await _db.HostProperties.FirstOrDefaultAsync(p => p.Id == propertyId && p.IsActive);
        if (property == null) return NotFound();

        string error = null;
        if (checkIn.Date < DateTime.Today) error = "Check-in cannot be in the past.";
        else if (checkOut.Date <= checkIn.Date) error = "Check-out must be after check-in.";
        else if (guests < 1 || guests > property.MaxGuests) error = "This property sleeps up to " + property.MaxGuests + " guests.";
        else
        {
            bool taken = await _db.Bookings.AnyAsync(b => b.PropertyId == propertyId
                && (b.Status == BookingStatus.Approved || b.Status == BookingStatus.CheckedIn)
                && checkIn.Date < b.CheckOutDate && checkOut.Date > b.CheckInDate);
            if (taken) error = "Those dates are already booked.";
        }

        if (error != null)
        {
            TempData["Error"] = error;
            return RedirectToAction("Details", "Home", new { id = propertyId });
        }

        // TotalPrice = (PricePerNight x Nights) + CleaningFee + ServiceFee
        int nights = (checkOut.Date - checkIn.Date).Days;
        decimal roomTotal = property.PricePerNight * nights;
        decimal serviceFee = Math.Round(roomTotal * BookingRules.ServiceFeeRate, 2);
        decimal totalPrice = roomTotal + property.CleaningFee + serviceFee;

        var userId = _users.GetUserId(User);
        var booking = new Booking
        {
            PropertyId = propertyId,
            GuestId = userId,
            CheckInDate = checkIn.Date,
            CheckOutDate = checkOut.Date,
            NumberOfGuests = guests,
            TotalPrice = totalPrice,
            Status = BookingStatus.Pending,
            Payment = new Payment { Amount = totalPrice, Status = PaymentStatus.Pending }
        };
        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync();

        await _audit.LogAsync(userId, "Requested booking #" + booking.Id + " for " + property.Title, ActivityType.Booking);
        await _audit.NotifyAsync(property.HostId, "New booking request", "A guest requested '" + property.Title + "'.", NotificationType.Warning, "/Host/BookingRequests");
        await _audit.NotifyAsync(userId, "Booking requested", "Your request for '" + property.Title + "' is waiting for the host.", NotificationType.Info, "/Booking/MyBookings");

        return RedirectToAction("MyBookings");
    }

    public async Task<IActionResult> MyBookings()
    {
        var userId = _users.GetUserId(User);
        var bookings = await _db.Bookings
            .Include(b => b.Property)
            .Include(b => b.Payment)
            .Where(b => b.GuestId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
        return View(bookings);
    }

    // ---------- Guest check-in: 3 steps (1 read rules, 2 upload document, 3 submit) ----------

    public async Task<IActionResult> CheckIn(int id, int step = 1)
    {
        var booking = await LoadBookingAsync(id);
        if (booking == null) return NotFound();

        if (step < 1) step = 1;
        if (step > 3) step = 3;
        var documents = booking.GuestCheckIn.Documents;
        if (step == 3 && documents.Count == 0) step = 2;   // must upload before submitting

        ViewBag.Step = step;
        return View(booking);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadDocument(int id, string documentType, IFormFile file)
    {
        var booking = await LoadBookingAsync(id);
        if (booking == null) return NotFound();

        if (!FileHelper.IsValid(file, FileHelper.DocumentTypes))
        {
            TempData["Error"] = "Please choose a jpg, png or pdf file (max 5MB).";
            return RedirectToAction("CheckIn", new { id, step = 2 });
        }

        var folder = Path.Combine(_env.ContentRootPath, "App_Data", "documents");
        var fileName = await FileHelper.SaveAsync(file, folder);

        booking.GuestCheckIn.Documents.Add(new GuestDocument
        {
            DocumentType = documentType == "Passport" ? "Passport" : "ID",
            FileName = fileName
        });
        booking.GuestCheckIn.Status = CheckInStatus.InProgress;
        await _db.SaveChangesAsync();

        await _audit.LogAsync(booking.GuestId, "Uploaded a check-in document for booking #" + booking.Id, ActivityType.CheckIn);
        return RedirectToAction("CheckIn", new { id, step = 3 });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitCheckIn(int id)
    {
        var booking = await LoadBookingAsync(id);
        if (booking == null) return NotFound();

        if (booking.GuestCheckIn.Documents.Count == 0)
            return RedirectToAction("CheckIn", new { id, step = 2 });

        booking.GuestCheckIn.Status = CheckInStatus.Completed;
        await _db.SaveChangesAsync();

        await _audit.LogAsync(booking.GuestId, "Submitted check-in for booking #" + booking.Id, ActivityType.CheckIn);
        await _audit.NotifyAsync(booking.Property.HostId, "Check-in ready to verify",
            "A guest submitted documents for '" + booking.Property.Title + "'.", NotificationType.Warning, "/Host/CheckIns");

        return RedirectToAction("MyBookings");
    }


    // Loads one of MY bookings that has been approved (so a check-in exists).
    private async Task<Booking> LoadBookingAsync(int id)
    {
        var userId = _users.GetUserId(User);
        return await _db.Bookings
            .Include(b => b.Property).ThenInclude(p => p.CheckInProcess)
            .Include(b => b.GuestCheckIn).ThenInclude(g => g.Documents)
            .FirstOrDefaultAsync(b => b.Id == id && b.GuestId == userId
                && b.GuestCheckIn != null
                && (b.Status == BookingStatus.Approved || b.Status == BookingStatus.CheckedIn));
    }

    // adding a review

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddReview(int propertyId, int rating, string comment)
    {
        var userId = _users.GetUserId(User);
        bool stayed = await _db.Bookings.AnyAsync(b => b.PropertyId == propertyId && b.GuestId == userId && b.Status == BookingStatus.CheckedIn);
        bool reviewed = await _db.Reviews.AnyAsync(r => r.PropertyId == propertyId && r.ReviewerId == userId);

        if (stayed && !reviewed && rating >= 1 && rating <= 5)
        {
            _db.Reviews.Add(new Review { PropertyId = propertyId, ReviewerId = userId, Rating = rating, Comment = comment ?? "" });
            await _db.SaveChangesAsync();
        }
        return RedirectToAction("Details", "Home", new { id = propertyId });
    }
}
