using System.ComponentModel.DataAnnotations;

namespace Kartverket.Database.Models;

public class Messages
{
    [Key] public int MessageId { get; set; }
    [MaxLength(256)]
    public string Message { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}