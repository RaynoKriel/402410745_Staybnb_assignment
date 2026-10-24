using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using _402410745_Staybnb_assignment.Data;
using _402410745_Staybnb_assignment.Models;
using _402410745_Staybnb_assignment.Services;
using _402410745_Staybnb_assignment.ViewModels;

namespace _402410745_Staybnb_assignment.Controllers;

// Host-only pages.
[Authorize(Roles = "Host")]
public class HostController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _users;
    private readonly AuditService _audit;

    public HostController(ApplicationDbContext db, UserManager<ApplicationUser> users, AuditService audit)
    {
        _db = db;
        _users = users;
        _audit = audit;
    }

    // Only the properties whose application an Admin has approved.
    private IQueryable<HostProperty> ApprovedPropertiesOf(string userId)
    {
        return _db.HostProperties.Where(p => p.HostId == userId && _db.HostApplications.Any(a => a.PropertyId == p.Id && a.Status == ApplicationStatus.Approved));
    }

    public async Task<IActionResult> Dashboard()
    {
        var userId = _users.GetUserId(User);
        var properties = ApprovedPropertiesOf(userId);
        var bookings = _db.Bookings.Where(b => b.Property.HostId == userId);
        var reviews = _db.Reviews.Where(r => r.Property.HostId == userId);

        var model = new HostDashboardViewModel
        {
            TotalProperties = await properties.CountAsync(),
            ActiveProperties = await properties.CountAsync(p => p.IsActive),
            TotalBookings = await bookings.CountAsync(),
            PendingBookings = await bookings.CountAsync(b => b.Status == BookingStatus.Pending),
            TotalEarnings = await _db.Payments.Where(p => p.Status == PaymentStatus.Paid && p.Booking.Property.HostId == userId).SumAsync(p => p.Amount),
            ReviewCount = await reviews.CountAsync()
        };
        if (model.ReviewCount > 0)
            model.AverageRating = await reviews.AverageAsync(r => r.Rating);

        return View(model);
    }

    public async Task<IActionResult> MyProperties()
    {
        var userId = _users.GetUserId(User);
        var list = await ApprovedPropertiesOf(userId).Include(p => p.Images).Include(p => p.Bookings).OrderByDescending(p => p.CreatedAt).ToListAsync();
        return View(list);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var userId = _users.GetUserId(User);
        var property = await ApprovedPropertiesOf(userId).FirstOrDefaultAsync(p => p.Id == id);
        if (property == null) return NotFound();

        property.IsActive = !property.IsActive;
        await _db.SaveChangesAsync();

        await _audit.LogAsync(userId, (property.IsActive ? "Activated " : "Deactivated ") + property.Title, ActivityType.Property);
        return RedirectToAction("MyProperties");
    }

    // Request made for a booking

    public async Task<IActionResult> BookingRequests()
    {
        var userId = _users.GetUserId(User);
        var bookings = await _db.Bookings.Include(b => b.Property).Include(b => b.Guest).Where(b => b.Property.HostId == userId).OrderByDescending(b => b.CreatedAt).ToListAsync();
        return View(bookings);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DecideBooking(int id, bool approve)
    {
        var userId = _users.GetUserId(User);
        var booking = await _db.Bookings.Include(b => b.Property).ThenInclude(p => p.CheckInProcess).Include(b => b.Payment)
            .FirstOrDefaultAsync(b => b.Id == id && b.Property.HostId == userId && b.Status == BookingStatus.Pending);
        if (booking == null) return NotFound();

        if (approve)
        {
            bool taken = await _db.Bookings.AnyAsync(b => b.Id != booking.Id && b.PropertyId == booking.PropertyId
                && (b.Status == BookingStatus.Approved || b.Status == BookingStatus.CheckedIn)
                && booking.CheckInDate < b.CheckOutDate && booking.CheckOutDate > b.CheckInDate);
            if (taken)
            {
                TempData["Error"] = "Those dates overlap with another approved booking. Please choose other dates";
                return RedirectToAction("BookingRequests");
            }

            booking.Status = BookingStatus.Approved;
            booking.Payment.Status = PaymentStatus.Paid;

            // Approval creates the guest's check-in record (Booking 1-to-1 GuestCheckIn).
            var process = booking.Property.CheckInProcess;
            if (process == null)
            {
                process = CheckInProcess.CreateDefault();
                process.PropertyId = booking.PropertyId;
            }
            _db.GuestCheckIns.Add(new GuestCheckIn { Booking = booking, CheckInProcess = process });
        }
        else
        {
            booking.Status = BookingStatus.Rejected;
            booking.Payment.Status = PaymentStatus.Failed;
        }
        await _db.SaveChangesAsync();

        string word = approve ? "approved" : "rejected";
        await _audit.LogAsync(userId, "Booking #" + booking.Id + " " + word, ActivityType.Booking);
        await _audit.NotifyAsync(booking.GuestId, approve ? "Booking Approved" : "Booking Rejected",
            "Your booking for '" + booking.Property.Title + "' was " + word + ".",
            approve ? NotificationType.Success : NotificationType.Warning, "/Booking/MyBookings");

        return RedirectToAction("BookingRequests");
    }

    // Process for checkin

    [HttpGet]
    public async Task<IActionResult> ConfigureCheckIn(int id)
    {
        var userId = _users.GetUserId(User);
        var property = await ApprovedPropertiesOf(userId).Include(p => p.CheckInProcess).FirstOrDefaultAsync(p => p.Id == id);
        if (property == null) return NotFound();

        ViewBag.PropertyTitle = property.Title;
        return View(property.CheckInProcess ?? CheckInProcess.CreateDefault());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfigureCheckIn(int id, string title, string rules)
    {
        var userId = _users.GetUserId(User);
        var property = await ApprovedPropertiesOf(userId).Include(p => p.CheckInProcess).FirstOrDefaultAsync(p => p.Id == id);
        if (property == null) return NotFound();

        if (property.CheckInProcess == null)
            property.CheckInProcess = new CheckInProcess();

        property.CheckInProcess.Title = string.IsNullOrWhiteSpace(title) ? "Check-in" : title;
        var lines = (rules ?? "").Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        property.CheckInProcess.SetSteps(lines);
        await _db.SaveChangesAsync();

        await _audit.LogAsync(userId, "Updated check-in process for " + property.Title, ActivityType.Property);
        TempData["Message"] = "Check-in process saved.";
        return RedirectToAction("MyProperties");
    }

    // checking the uploaded documents for proof

    public async Task<IActionResult> CheckIns()
    {
        var userId = _users.GetUserId(User);
        var bookings = await _db.Bookings.Include(b => b.Property).Include(b => b.Guest).Include(b => b.GuestCheckIn).ThenInclude(g => g.Documents)
            .Where(b => b.Property.HostId == userId && b.GuestCheckIn != null && (b.Status == BookingStatus.Approved || b.Status == BookingStatus.CheckedIn))
            .OrderByDescending(b => b.CheckInDate).ToListAsync();
        return View(bookings);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyDocument(int id, bool verify)
    {
        var userId = _users.GetUserId(User);
        var document = await _db.GuestDocuments.Include(d => d.GuestCheckIn).ThenInclude(c => c.Documents).Include(d => d.GuestCheckIn).ThenInclude(c => c.Booking).ThenInclude(b => b.Property)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (document == null) return NotFound();
        var checkIn = document.GuestCheckIn;
        var booking = checkIn.Booking;
        if (booking.Property.HostId != userId) return Forbid();

        if (verify)
        {
            document.Status = DocumentStatus.Verified;

            // When the check-in is submitted and every document is verified the guest is checked in.
            bool allVerified = checkIn.Documents.All(d => d.Status == DocumentStatus.Verified);
            if (allVerified && checkIn.Status == CheckInStatus.Completed)
            {
                booking.Status = BookingStatus.CheckedIn;
                await _audit.NotifyAsync(booking.GuestId, "Checked in", "You are checked in at '" + booking.Property.Title + "'.", NotificationType.Success, "/Booking/MyBookings");
            }
        }
        else
        {
            document.Status = DocumentStatus.Rejected;
            checkIn.Status = CheckInStatus.InProgress;   // guest must upload a new document
            await _audit.NotifyAsync(booking.GuestId, "Document rejected", "Please upload a new document for '" + booking.Property.Title + "'.", NotificationType.Warning, "/Booking/CheckIn/" + booking.Id + "?step=2");
        }
        await _db.SaveChangesAsync();

        await _audit.LogAsync(userId, (verify ? "Verified" : "Rejected") + " a document for booking #" + booking.Id, ActivityType.CheckIn);
        return RedirectToAction("CheckIns");
    }


}
