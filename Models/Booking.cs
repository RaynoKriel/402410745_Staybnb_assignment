using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _402410745_Staybnb_assignment.Models;

public class Booking
{
    public int Id { get; set; }

    public int PropertyId { get; set; }
    public HostProperty Property { get; set; }

    [Required]
    public string GuestId { get; set; }
    public ApplicationUser Guest { get; set; }

    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int NumberOfGuests { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalPrice { get; set; }

    public BookingStatus Status { get; set; } = BookingStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public Payment Payment { get; set; }            // one-to-one
    public GuestCheckIn GuestCheckIn { get; set; }  // one-to-one

    public int Nights => (CheckOutDate - CheckInDate).Days;
}
