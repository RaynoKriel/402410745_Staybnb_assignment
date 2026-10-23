using System.ComponentModel.DataAnnotations;

namespace _402410745_Staybnb_assignment.Models;

public class WishlistItem
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }

    public int PropertyId { get; set; }
    public HostProperty Property { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
