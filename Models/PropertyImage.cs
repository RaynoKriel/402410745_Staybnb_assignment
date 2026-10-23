using System.ComponentModel.DataAnnotations;

namespace _402410745_Staybnb_assignment.Models;

public class PropertyImage
{
    public int Id { get; set; }

    public int PropertyId { get; set; }
    public HostProperty Property { get; set; }

    [Required]
    public string ImageUrl { get; set; }   // e.g. /uploads/abc123.jpg
}
