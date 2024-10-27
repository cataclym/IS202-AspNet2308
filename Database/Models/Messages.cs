using System.ComponentModel.DataAnnotations;

namespace Kartverket.Database.Models;

public class Messages
{
    [Key] public int MessageId { get; set; }
    
    [Required]
    [MinLength(5)]
    [MaxLength(256)]
    public string Message { get; set; } // MinLength fungerer kun på strenger, arrays, eller samlinger
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}