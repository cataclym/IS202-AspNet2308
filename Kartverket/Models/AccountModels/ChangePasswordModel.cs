using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Kartverket.Models
{
    public class ChangePasswordModel
    {
        [Required(ErrorMessage = "Nåværende passord er påkrevd.")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }
        
        [Required(ErrorMessage = "Nytt passord er påkrevd.")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Passordet må være minst 6 tegn langt.")]
        public string NewPassword { get; set; }
        
        [Required(ErrorMessage = "Bekreft nytt passord er påkrevd.")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Passordene samsvarer ikke.")]
        public string ConfirmPassword { get; set; }
        
    }
}
