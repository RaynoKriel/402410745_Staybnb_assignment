using System.ComponentModel.DataAnnotations;
using _402410745_Staybnb_assignment.Models;

namespace _402410745_Staybnb_assignment.ViewModels;

public class RegisterViewModel
{
    [Required, StringLength(50)]
    public string FirstName { get; set; }

    [Required, StringLength(50)]
    public string LastName { get; set; }

    [Required, EmailAddress]
    public string Email { get; set; }

    [Required, DataType(DataType.Password)]
    public string Password { get; set; }

    [Required, DataType(DataType.Password), Compare("Password", ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; }
}

public class LoginViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; }

    [Required, DataType(DataType.Password)]
    public string Password { get; set; }

    public bool RememberMe { get; set; }
}

public class HostApplyViewModel
{
    [Required, StringLength(100)]
    public string Title { get; set; }

    [Required]
    public string Description { get; set; }

    [Range(1, 1000000, ErrorMessage = "Enter a price per night.")]
    public decimal PricePerNight { get; set; }

    [Range(0, 1000000)]
    public decimal CleaningFee { get; set; }

    [Required, StringLength(250)]
    public string Address { get; set; }

    [Required, StringLength(100)]
    public string City { get; set; }

    [Required]
    public string PropertyType { get; set; } = "Apartment";

    [Range(1, 50)]
    public int MaxGuests { get; set; } = 2;
    [Range(0, 50)]
    public int Bedrooms { get; set; } = 1;
    [Range(0, 50)]
    public int Beds { get; set; } = 1;
    [Range(0, 50)]
    public int Bathrooms { get; set; } = 1;

    public List<int> AmenityIds { get; set; } = new();
    public List<Amenity> AllAmenities { get; set; } = new();
}

public class HostDashboardViewModel
{
    public int TotalProperties { get; set; }
    public int ActiveProperties { get; set; }
    public int TotalBookings { get; set; }
    public int PendingBookings { get; set; }
    public decimal TotalEarnings { get; set; }
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
}

public class AdminDashboardViewModel
{
    public int TotalUsers { get; set; }
    public int PendingApplications { get; set; }
    public int ActiveProperties { get; set; }
    public decimal PlatformRevenue { get; set; }
    public int TotalBookings { get; set; }
    public int ApprovedHosts { get; set; }
    public List<ActivityLog> RecentActivity { get; set; } = new();
    public List<HostApplication> RecentApplications { get; set; } = new();
    public List<Booking> RecentBookings { get; set; } = new();
}

