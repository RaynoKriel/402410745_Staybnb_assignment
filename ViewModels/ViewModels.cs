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

