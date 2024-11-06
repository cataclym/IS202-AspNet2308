using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Kartverket.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Kartverket.Models;

public class MyPageModel
{
    public int UserId { get; set; }
    
    public string? Username { get; set; }
        
    public string? Email { get; set; }
    
    public string? Phone { get; set; }
    
}

   
