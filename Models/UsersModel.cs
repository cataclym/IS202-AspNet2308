using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Kartverket.Models;

// Data tilhørende login form
[PrimaryKey("UserId")] // Denne linjen angir at UserId er primærnøkkelen.
public class Users
{
    [Key] public int UserId { get; set; }

    [Required]
    [PasswordPropertyText]
    [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Brukernavnet må kun bestå av bokstaver og tall.")]
    public string? UserName { get; set; }

    [Required]
    [PasswordPropertyText]
    [DataType(DataType.Password)]
    public string? Password { get; set; }

    [Required] [EmailAddress] public string? Email { get; set; }
    [Phone] public string? Phone { get; set; }
}