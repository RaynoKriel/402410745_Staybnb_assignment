using System.ComponentModel.DataAnnotations;

namespace _402410745_Staybnb_assignment.Models;

public class GuestDocument
{
    public int Id { get; set; }

    public int GuestCheckInId { get; set; }
    public GuestCheckIn GuestCheckIn { get; set; }

    [Required, StringLength(100)]
    public string DocumentType { get; set; }   // "ID" or "Passport"

    [Required, StringLength(200)]
    public string FileName { get; set; }       // saved file name (not publicly reachable)

    public DocumentStatus Status { get; set; } = DocumentStatus.Pending;
}
