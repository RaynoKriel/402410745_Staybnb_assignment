using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using _402410745_Staybnb_assignment.Data;
using _402410745_Staybnb_assignment.Models;

namespace Staybnb.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _users;

    public HomeController(ApplicationDbContext db, UserManager<ApplicationUser> users)
    {
        _db = db;
        _users = users;
    }

    // Public gallery of active properties, with city / type filters.
    public async Task<IActionResult> Index(string city, string type)
    {
        var query = _db.HostProperties.Include(p => p.Images).Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(city))
            query = query.Where(p => p.City.Contains(city));
        if (!string.IsNullOrWhiteSpace(type))
            query = query.Where(p => p.PropertyType == type);

        ViewBag.City = city;
        ViewBag.Type = type;
        var properties = await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
        return View(properties);
    }

    public async Task<IActionResult> Details(int id)
    {
        var property = await _db.HostProperties
            .Include(p => p.Images)
            .Include(p => p.Amenities)
            .Include(p => p.Host)
            .Include(p => p.Reviews).ThenInclude(r => r.Reviewer)
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

        if (property == null) return NotFound();

        // A guest may leave one review after a completed check-in.
        var userId = _users.GetUserId(User);
        bool canReview = false;
        if (userId != null && User.IsInRole("Guest"))
        {
            bool stayed = await _db.Bookings.AnyAsync(b => b.PropertyId == id && b.GuestId == userId && b.Status == BookingStatus.CheckedIn);
            bool reviewed = await _db.Reviews.AnyAsync(r => r.PropertyId == id && r.ReviewerId == userId);
            canReview = stayed && !reviewed;
        }
        ViewBag.CanReview = canReview;

        return View(property);
    }

    public IActionResult Error() => View();
}
