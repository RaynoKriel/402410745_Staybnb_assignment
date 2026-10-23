using System.ComponentModel.DataAnnotations;

namespace _402410745_Staybnb_assignment.Models;

public class Review
{
    public int Id { get; set; }

    public int PropertyId { get; set; }
    public HostProperty Property { get; set; }

    [Required]
    public string ReviewerId { get; set; }
    public ApplicationUser Reviewer { get; set; }

    [Range(1, 5)]
    public int Rating { get; set; }

    [StringLength(1000)]
    public string Comment { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
