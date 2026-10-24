using _402410745_Staybnb_assignment.Data;
using _402410745_Staybnb_assignment.Models;

namespace _402410745_Staybnb_assignment.Services;

// Two helpers used by every controller: write an ActivityLog row, or send a Notification.
public class AuditService
{
    private readonly ApplicationDbContext _db;

    public AuditService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task LogAsync(string userId, string action, ActivityType type)
    {
        _db.ActivityLogs.Add(new ActivityLog { UserId = userId, Action = action, ActivityType = type });
        await _db.SaveChangesAsync();
    }

    public async Task NotifyAsync(string userId, string title, string message,
        NotificationType type = NotificationType.Info, string link = null)
    {
        _db.Notifications.Add(new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            Link = link
        });
        await _db.SaveChangesAsync();
    }
}
