using System.ComponentModel.DataAnnotations;

namespace _402410745_Staybnb_assignment.Models;

public class Notification
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }

    [Required, StringLength(100)]
    public string Title { get; set; }

    [StringLength(500)]
    public string Message { get; set; }

    public NotificationType Type { get; set; } = NotificationType.Info;
    public bool IsRead { get; set; }

    [StringLength(250)]
    public string Link { get; set; }   // page to open when the notification is clicked

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
