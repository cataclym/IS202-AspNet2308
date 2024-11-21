using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Kartverket.Database.Models;

[PrimaryKey("UserId")] // Denne linjen angir at UserId er primærnøkkelen.
public class Users
{
    [Key] public int UserId { get; set; }
    [MaxLength(45)]
    public string Username { get; set; }
    [MaxLength(72)]
    public string Password { get; set; }
    [MaxLength(50)]
    public string Email { get; set; }
    [MaxLength(15)] // Valgfritt felt
    public string? Phone { get; set; }
    public bool IsAdmin { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    
    public ICollection<Messages> Messages { get; set; } = new List<Messages>();
    public ICollection<Reports> Reports { get; set; } = new List<Reports>();
    public ICollection<Reports> AssignedReports { get; set; } // Reports assigned to the admin
    public ICollection<PinnedReport> PinnedReports { get; set; }
    
}

