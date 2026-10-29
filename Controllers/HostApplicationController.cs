using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using _402410745_Staybnb_assignment.Data;
using _402410745_Staybnb_assignment.Models;
using _402410745_Staybnb_assignment.Services;
using _402410745_Staybnb_assignment.ViewModels;

namespace _402410745_Staybnb_assignment.Controllers;

// "Become a Host" - a Guest (or an existing Host adding another property) submits a property.
[Authorize(Roles = "Guest,Host")]
public class HostApplicationController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _users;
    private readonly AuditService _audit;
    private readonly IWebHostEnvironment _env;

    public HostApplicationController(ApplicationDbContext db, UserManager<ApplicationUser> users,
        AuditService audit, IWebHostEnvironment env)
    {
        _db = db;
        _users = users;
        _audit = audit;
        _env = env;
    }

    // The user's own applications and their status
    public async Task<IActionResult> Index()
    {
        var userId = _users.GetUserId(User);
        var applications = await _db.HostApplications.Include(a => a.Property).Where(a => a.ApplicationUserId == userId).OrderByDescending(a => a.AppliedAt).ToListAsync();
        return View(applications);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new HostApplyViewModel { 
            AllAmenities = await _db.Amenities.ToListAsync() 
        };
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(HostApplyViewModel model, List<IFormFile> images)
    {
        images ??= new List<IFormFile>();
        var validImages = images.Where(i => FileHelper.IsValid(i, FileHelper.ImageTypes)).ToList();

        // Rule: an application cannot be submitted without at least one image.
        if (validImages.Count == 0)
        {
            ModelState.AddModelError("", "Please upload at least one property image (jpg, png or webp, max 5MB each).");
        }
        if (!ModelState.IsValid)
        {
            model.AllAmenities = await _db.Amenities.ToListAsync();
            return View(model);
        }

        var userId = _users.GetUserId(User);

        var property = new HostProperty
        {
            Title = model.Title,
            Description = model.Description,
            PricePerNight = model.PricePerNight,
            CleaningFee = model.CleaningFee,
            Address = model.Address,
            City = model.City,
            PropertyType = model.PropertyType,
            MaxGuests = model.MaxGuests,
            Bedrooms = model.Bedrooms,
            Beds = model.Beds,
            Bathrooms = model.Bathrooms,
            HostId = userId,
            IsActive = false
        };

        property.Amenities = await _db.Amenities.Where(a => model.AmenityIds.Contains(a.Id)).ToListAsync();

        var uploadFolder = Path.Combine(_env.WebRootPath, "uploads");
        foreach (var image in validImages)
        {
            var fileName = await FileHelper.SaveAsync(image, uploadFolder);
            property.Images.Add(new PropertyImage { ImageUrl = "/uploads/" + fileName });
        }

        // Property + images + application are saved together in one SaveChanges.
        _db.HostApplications.Add(new HostApplication { ApplicationUserId = userId, Property = property });
        await _db.SaveChangesAsync();

        await _audit.LogAsync(userId, "Submitted host application: " + property.Title, ActivityType.Application);
        await _audit.NotifyAsync(userId, "Application submitted", "Your application for '" + property.Title + "' is under review.", 
            NotificationType.Info, 
            "/HostApplication");

        // Tell every Admin / SuperAdmin that something is waiting.
        var admins = new List<ApplicationUser>();
        admins.AddRange(await _users.GetUsersInRoleAsync("Admin"));
        admins.AddRange(await _users.GetUsersInRoleAsync("SuperAdmin"));
        foreach (var admin in admins)
            await _audit.NotifyAsync(admin.Id, "New host application", "'" + property.Title + "' needs review.",
                NotificationType.Warning, 
                "/Admin/Applications");

        return RedirectToAction("Index");
    }
}
