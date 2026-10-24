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
    
    

}
