using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Kartverk.Models;

// Data tilhørende login form
public class LoginData
{
    [Required]
    [PasswordPropertyText]
    public string? UserName { get; set; }
    [Required]
    [PasswordPropertyText]
    public string? Password { get; set; }
}
