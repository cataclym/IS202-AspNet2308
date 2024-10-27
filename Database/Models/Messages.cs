using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kartverket.Database.Models;

public class Messages
{
    [Key] public int MessageId { get; set; }
    
    [Required]
    [MinLength(5)]
    [MaxLength(256)]
    public string Message { get; set; } // MinLength fungerer kun på strenger, arrays, eller samlinger
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    // Foreign key property
    public int UserId { get; set; }

    // Navigation property to User
    [ForeignKey("UserId")]
    public Users User { get; set; }
}