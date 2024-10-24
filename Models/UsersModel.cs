using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Kartverket.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Kartverket.Models;

// Data tilhørende login form
[PrimaryKey("UserId")] // Denne linjen angir at UserId er primærnøkkelen.
public class UsersModel
{
    [Key] public int UserId { get; set; }
    [Required]
    [PasswordPropertyText]
    [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Brukernavnet må kun bestå av bokstaver og tall.")]
    public string Username { get; set; }
    [Required]
    [PasswordPropertyText]
    [DataType(DataType.Password)]
    public string Password { get; set; }
    [EmailAddress] public string Email { get; set; }
    [Phone] public string? Phone { get; set; }
    public bool IsAdmin { get; set; } = false;
    public ICollection<Reports> MapReports { get; set; } = new List<Reports>();
    // Konverterer Users til UsersModel uten problemer fordi det er samme felt
    public static implicit operator Users(UsersModel users) => new()
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