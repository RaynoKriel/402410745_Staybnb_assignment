using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using _402410745_Staybnb_assignment.Data;
using _402410745_Staybnb_assignment.Models;
using _402410745_Staybnb_assignment.Services;
using _402410745_Staybnb_assignment.ViewModels;

namespace _402410745_Staybnb_assignment.Controllers;

// Admin and SuperAdmin pages. The Users pages are SuperAdmin only for promottions.
[Authorize(Roles = "Admin,SuperAdmin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _users;
    private readonly AuditService _audit;

    public AdminController(ApplicationDbContext db, UserManager<ApplicationUser> users, AuditService audit)
    {
        _db = db;
        _users = users;
        _audit = audit;
    }

    public async Task<IActionResult> Dashboard()
    {
        var model = new AdminDashboardViewModel
        {
            TotalUsers = await _users.Users.CountAsync(),
            PendingApplications = await _db.HostApplications.CountAsync(a => a.Status == ApplicationStatus.Pending),
            ActiveProperties = await _db.HostProperties.CountAsync(p => p.IsActive),
            PlatformRevenue = await _db.Payments.Where(p => p.Status == PaymentStatus.Paid).SumAsync(p => p.Amount),
            TotalBookings = await _db.Bookings.CountAsync(),
            ApprovedHosts = (await _users.GetUsersInRoleAsync("Host")).Count,
            RecentActivity = await _db.ActivityLogs.Include(l => l.User).OrderByDescending(l => l.CreatedAt).Take(8).ToListAsync(),
            RecentApplications = await _db.HostApplications.Include(a => a.ApplicationUser).Include(a => a.Property).OrderByDescending(a => a.AppliedAt).Take(5).ToListAsync(),
            RecentBookings = await _db.Bookings.Include(b => b.Guest).Include(b => b.Property).OrderByDescending(b => b.CreatedAt).Take(5).ToListAsync()
        };
        return View(model);
    }

    // Reviewing the application as a host

    public async Task<IActionResult> Applications()
    {
        var list = await _db.HostApplications.Include(a => a.ApplicationUser).Include(a => a.Property).ThenInclude(p => p.Images)
            .OrderBy(a => a.Status).ThenByDescending(a => a.AppliedAt).ToListAsync();
        return View(list);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DecideApplication(int id, bool approve)
    {
        var application = await _db.HostApplications.Include(a => a.Property).ThenInclude(p => p.CheckInProcess)
            .FirstOrDefaultAsync(a => a.Id == id && a.Status == ApplicationStatus.Pending);
        if (application == null) return NotFound();

        var adminId = _users.GetUserId(User);
        var applicant = await _users.FindByIdAsync(application.ApplicationUserId);

        if (approve)
        {
            application.Status = ApplicationStatus.Approved;
            application.Property.IsActive = true;
            if (application.Property.CheckInProcess == null)
                application.Property.CheckInProcess = CheckInProcess.CreateDefault();
            await _db.SaveChangesAsync();

            // THE TRANSITION: add the Host role and remove the Guest role. so that only one account exists
            if (!await _users.IsInRoleAsync(applicant, "Host"))
            {
                await _users.AddToRoleAsync(applicant, "Host");
                if (await _users.IsInRoleAsync(applicant, "Guest"))
                    await _users.RemoveFromRoleAsync(applicant, "Guest");
                await _audit.LogAsync(applicant.Id, "Role changed: Guest -> Host", ActivityType.RoleChange);
            }
            //notify as approved 
            await _audit.NotifyAsync(applicant.Id, "Host Application Approved",
                "Congratulations! '" + application.Property.Title + "' is now live.", 
                NotificationType.Success, 
                "/Host/Dashboard");
        }
        else
        {
            application.Status = ApplicationStatus.Rejected;
            await _db.SaveChangesAsync();
            await _audit.NotifyAsync(applicant.Id, "Host Application Rejected",
                "Your application for '" + application.Property.Title + "' was not approved.", 
                NotificationType.Warning, "/HostApplication");
        }

        await _audit.LogAsync(adminId, (approve ? "Approved" : "Rejected") + " application #" + application.Id, ActivityType.Application);
        return RedirectToAction("Applications");
    }

    // write a audit log for the process

    public async Task<IActionResult> ActivityLogs()
    {
        var logs = await _db.ActivityLogs.Include(l => l.User).OrderByDescending(l => l.CreatedAt).Take(300).ToListAsync();
        return View(logs);
    }

    //The SUperadmin managment of USers

    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Users(string search)
    {
        var guests = await _users.GetUsersInRoleAsync("Guest");
        if (!string.IsNullOrWhiteSpace(search))
        {
            guests = guests.Where(u => u.Email.Contains(search, StringComparison.OrdinalIgnoreCase) || u.FullName.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        ViewBag.Search = search;
        return View(guests.OrderBy(u => u.Email).ToList());
    }

    [Authorize(Roles = "SuperAdmin"), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Promote(string userId)
    {
        var user = await _users.FindByIdAsync(userId);

        // Only Guests can be promoted. and only the superuser can promote them to an admin role
        if (user == null || !await _users.IsInRoleAsync(user, "Guest")) return NotFound();
        await _users.AddToRoleAsync(user, "Admin");
        await _users.RemoveFromRoleAsync(user, "Guest");
        //still logging all changes inthe audit table
        await _audit.LogAsync(_users.GetUserId(User), "Promoted " + user.Email + " to Admin", ActivityType.RoleChange);
        await _audit.LogAsync(user.Id, "Role changed: Guest -> Admin", ActivityType.RoleChange);
        await _audit.NotifyAsync(user.Id, "You are now an Admin", "A SuperAdmin promoted you to Admin.", NotificationType.Success, "/Admin/Dashboard");

        return RedirectToAction("Users");
    }
}
