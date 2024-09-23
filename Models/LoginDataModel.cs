using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Kartverket.Models;

// Data tilhørende login form
public class LoginDataModel
{
    [Required]
    [PasswordPropertyText]
    public string? UserName { get; set; }
    [Required]
    [PasswordPropertyText]
    public string? Password { get; set; }
}
