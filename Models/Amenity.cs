using System.ComponentModel.DataAnnotations;

namespace _402410745_Staybnb_assignment.Models;

public class Amenity
{
    public int Id { get; set; }

    [Required, StringLength(50)]
    public string Name { get; set; }

    [StringLength(50)]
    public string IconClass { get; set; }

    public List<HostProperty> Properties { get; set; } = new();
}
