namespace _402410745_Staybnb_assignment.Models;

public class GuestCheckIn
{
    public int Id { get; set; }

    public int BookingId { get; set; }
    public Booking Booking { get; set; }

    public int CheckInProcessId { get; set; }
    public CheckInProcess CheckInProcess { get; set; }

    public CheckInStatus Status { get; set; } = CheckInStatus.NotStarted;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public List<GuestDocument> Documents { get; set; } = new();   // one-to-many
}
