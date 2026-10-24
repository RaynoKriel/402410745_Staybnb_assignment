using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using _402410745_Staybnb_assignment.Data;
using _402410745_Staybnb_assignment.Models;

namespace _402410745_Staybnb_assignment.Controllers;

// Guest ID/passport files are kept in App_Data (not public), so they are served here.
// Only the guest who uploaded the file and the host of that property may open it.
[Authorize]
public class DocumentsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _users;
    private readonly IWebHostEnvironment _env;

    public DocumentsController(ApplicationDbContext db, UserManager<ApplicationUser> users, IWebHostEnvironment env)
    {
        _db = db;
        _users = users;
        _env = env;
    }

    public async Task<IActionResult> Download(int id)
    {
        var document = await _db.GuestDocuments.Include(d => d.GuestCheckIn).ThenInclude(c => c.Booking).ThenInclude(b => b.Property)
            .FirstOrDefaultAsync(d => d.Id == id);
        if (document == null) return NotFound();

        var me = _users.GetUserId(User);
        var booking = document.GuestCheckIn.Booking;
        if (booking.GuestId != me && booking.Property.HostId != me) return Forbid();

        var path = Path.Combine(_env.ContentRootPath, "App_Data", "documents", document.FileName);
        if (!System.IO.File.Exists(path)) return NotFound();

        var extension = Path.GetExtension(path).ToLowerInvariant();
        string contentType = "image/jpeg";
        if (extension == ".pdf") contentType = "application/pdf";
        else if (extension == ".png") contentType = "image/png";

        return PhysicalFile(path, contentType);
    }
}
