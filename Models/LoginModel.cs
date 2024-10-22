using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Kartverket.Models;

public class LoginModel
{
    [Required]
    [PasswordPropertyText]
    [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Brukernavnet må kun bestå av bokstaver og tall.")]
    public string Username { get; set; }
    [Required]
    [PasswordPropertyText]
    [DataType(DataType.Password)]
    public string Password { get; set; }
}