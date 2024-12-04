using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Kartverket.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Kartverket.Models;

// Data tilhørende login form
[PrimaryKey("UserId")] // Denne linjen angir at UserId er primærnøkkelen.
public class UserRegistrationModel
{
    [Key] public int UserId { get; set; }
    [Required]
    [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Brukernavnet må kun bestå av bokstaver og tall.")]
    public required string Username { get; set; }
    [Required]
    [PasswordPropertyText]
    [DataType(DataType.Password)]
    public required string Password { get; set; }
    [EmailAddress] public required string Email { get; set; }
    [Phone] public string? Phone { get; set; }
    public bool IsAdmin { get; set; } = false;
    public ICollection<ReportViewModel> MapReports { get; set; } = new List<ReportViewModel>();
    // Konverterer Users til UsersModel uten problemer fordi det er samme felt
    public static implicit operator Users(UserRegistrationModel userRegistration) => new Users
    {
        UserId = userRegistration.UserId,
        Email = userRegistration.Email,
        IsAdmin = userRegistration.IsAdmin,
        Password = userRegistration.Password,
        Username = userRegistration.Username,
        Phone = userRegistration.Phone,
    };
    
    public static UserRegistrationModel FromUsers(Users user)
    {
        return new UserRegistrationModel
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