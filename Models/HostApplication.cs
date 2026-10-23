using System.ComponentModel.DataAnnotations;

namespace _402410745_Staybnb_assignment.Models;

// A Guest (or Host) applies by submitting a property. The property is saved as inactive
// and only goes live when an Admin approves this application.
public class HostApplication
{
    public int Id { get; set; }

    [Required]
    public string ApplicationUserId { get; set; }
    public ApplicationUser ApplicationUser { get; set; }

    public int PropertyId { get; set; }
    public HostProperty Property { get; set; }

    public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;
    public DateTime AppliedAt { get; set; } = DateTime.Now;
}
