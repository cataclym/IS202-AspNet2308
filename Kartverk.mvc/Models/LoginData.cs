using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Kartverk.mvc.Models;

// Data tilhørende login form
public class LoginData
{
    [Required]
    public string UserName { get; set; }
    [Required]
    [PasswordPropertyText]
    public string Password { get; set; }
}
