using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _402410745_Staybnb_assignment.Models;

public class Message
{
    public int Id { get; set; }

    [Required]
    public string SenderId { get; set; }
    [ForeignKey("SenderId")]
    public ApplicationUser Sender { get; set; }

    [Required]
    public string ReceiverId { get; set; }
    [ForeignKey("ReceiverId")]
    public ApplicationUser Receiver { get; set; }

    [Required, StringLength(2000)]
    public string Content { get; set; }

    public bool IsRead { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
}
