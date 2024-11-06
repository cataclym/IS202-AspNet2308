using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Kartverket.Models.AccountModels
{
    public class ChangePasswordModel : Controller
    {
        [Required(ErrorMessage = "Nåværende passord er påkrevd.")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "Nytt passord er påkrevd.")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Bekreft nytt passord er påkrevd.")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Passordene samsvarer ikke.")]
        public string ConfirmPassword { get; set; }
    }
}
