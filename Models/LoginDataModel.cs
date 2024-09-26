using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Kartverket.Models;

// Data tilhørende login form
public class LoginDataModel
{
    [Key] // Denne linjen angir at UserId er primærnøkkelen.
    public int UserId { get; set; }
    
    [Required]
    [PasswordPropertyText]
    public string? UserName { get; set; }
    [Required]
    [PasswordPropertyText]
    public string? Password { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    
}
