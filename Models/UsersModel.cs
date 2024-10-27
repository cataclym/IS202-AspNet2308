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
    public ICollection<ReportViewModel> MapReports { get; set; } = new List<ReportViewModel>();
    // Konverterer Users til UsersModel uten problemer fordi det er samme felt
    public static implicit operator Users(UsersModel users) => new Users
    {
        UserId = users.UserId,
        Email = users.Email,
        IsAdmin = users.IsAdmin,
        Password = users.Password,
        Username = users.Username,
        Phone = users.Phone,
    };
    
    public static UsersModel FromUsers(Users user)
    {
        return new UsersModel
        {
            UserId = user.UserId,
            Username = user.Username,
            Password = user.Password,
            Email = user.Email,
            Phone = user.Phone,
            IsAdmin = user.IsAdmin,
            // Kopiere eller tilordne andre nødvendige egenskaper her
        };
    }

}