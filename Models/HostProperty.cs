using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _402410745_Staybnb_assignment.Models;

public class HostProperty
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Title { get; set; }

    [Required]
    public string Description { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal PricePerNight { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal CleaningFee { get; set; }

    [Required, StringLength(250)]
    public string Address { get; set; }

    [StringLength(100)]
    public string City { get; set; }

    [StringLength(50)]
    public string PropertyType { get; set; }

    public int MaxGuests { get; set; }
    public int Bedrooms { get; set; }
    public int Beds { get; set; }
    public int Bathrooms { get; set; }

    // A property only becomes active once an Admin approves the HostApplication.
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Required]
    public string HostId { get; set; }
    public ApplicationUser Host { get; set; }

    public List<PropertyImage> Images { get; set; } = new();
    public List<Amenity> Amenities { get; set; } = new();      // many-to-many
    public List<Booking> Bookings { get; set; } = new();
    public List<Review> Reviews { get; set; } = new();
    public CheckInProcess CheckInProcess { get; set; }          // one-to-one
}
