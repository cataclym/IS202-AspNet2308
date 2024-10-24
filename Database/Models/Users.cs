using System.ComponentModel.DataAnnotations;
using Kartverket.Models;
using Microsoft.EntityFrameworkCore;

namespace Kartverket.Database.Models;

[PrimaryKey("UserId")] // Denne linjen angir at UserId er primærnøkkelen.
public class Users
{
    [Key] public int UserId { get; set; }
    [MaxLength(45)]
    public string Username { get; set; }
    [MaxLength(45)]
    public string Password { get; set; }
    [MaxLength(50)]
    public string Email { get; set; }
    [MaxLength(15)] // Valgfritt felt
    public string? Phone { get; set; }
    public bool IsAdmin { get; set; } = false;
    public DateTime CreatedAt { get; set; }  = DateTime.Now;
    public ICollection<Reports> MapReports { get; set; } = new List<Reports>();

    // Konverterer Users til UsersModel uten problemer fordi det er samme felt
    public static implicit operator UsersModel(Users users) => new()
    {
        UserId = users.UserId,
        Email = users.Email,
        IsAdmin = users.IsAdmin,
        MapReports = users.MapReports,
        Password = users.Password,
        Username = users.Username,
        Phone = users.Phone,
    };
}