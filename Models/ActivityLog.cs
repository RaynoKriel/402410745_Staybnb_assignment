using System.ComponentModel.DataAnnotations;

namespace _402410745_Staybnb_assignment.Models;

public class ActivityLog
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }

    [Required, StringLength(100)]
    public string Action { get; set; }

    public ActivityType ActivityType { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
