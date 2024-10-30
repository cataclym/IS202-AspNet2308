using System.ComponentModel.DataAnnotations;
using Kartverket.Models;

namespace Kartverket.Models
{
    public class MessagesModel
    {
        [Key] // Dette angir at MessageId er primærnøkkelen for MessagesModel
        public int MessageId { get; set; }

        [Required]
        [MaxLength(500)] // Eksempel på begrensning, tilpass etter behov
        public string Message { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Legg til Username for å vise brukernavn i visningen
        public string Username { get; set; }

        // Hvis meldinger har en relasjon til en bruker eller rapport
        public int UserId { get; set; }
        public UserRegistrationModel UserRegistration { get; set; }

        // Legg til andre nødvendige felter og relasjoner her...
    }
}