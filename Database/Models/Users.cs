using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Kartverket.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Kartverket.Database.Models;

[PrimaryKey("UserId")] // Denne linjen angir at UserId er primærnøkkelen.
public class Users
{
    [Key] public int UserId { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public bool IsAdmin { get; set; } = false;
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