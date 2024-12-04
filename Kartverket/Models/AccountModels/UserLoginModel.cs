using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Kartverket.Models;

public class UserLoginModel
{
    [Required]
    [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Brukernavnet må kun bestå av bokstaver og tall.")]
    public required string Username { get; set; }
    [Required]
    [PasswordPropertyText]
    [DataType(DataType.Password)]
    public required string Password { get; set; }
}