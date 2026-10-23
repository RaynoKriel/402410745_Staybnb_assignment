using Microsoft.AspNetCore.Identity;

namespace _402410745_Staybnb_assignment.Models;

// IdentityUser already has Id, Email, PasswordHash, etc. We only add our extra fields.
public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string ProfilePictureUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public string FullName => FirstName + " " + LastName;
}
