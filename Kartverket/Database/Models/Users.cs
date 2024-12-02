using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Kartverket.Database.Models;

[PrimaryKey("UserId")] // Denne linjen angir at UserId er primærnøkkelen.
public class Users
{
    [Key] public int UserId { get; set; }
    [MaxLength(45)]
    public required string Username { get; set; }
    [MaxLength(72)]
    public required string Password { get; set; }
    [MaxLength(50)]
    public required string Email { get; set; }
    [MaxLength(15)] // Valgfritt felt
    public string? Phone { get; set; }
    public bool IsAdmin { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public ICollection<Messages> Messages { get; set; } = new List<Messages>();
    public ICollection<Reports> Reports { get; set; } = new List<Reports>();
    // Reports assigned to the admin
    public ICollection<Reports> AssignedReports { get; set; } = new List<Reports>(); 
    public ICollection<PinnedReport> PinnedReports { get; set; } = new List<PinnedReport>();
    
}

