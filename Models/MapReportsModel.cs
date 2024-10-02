using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kartverket.Models;

[Table("Reports")]
public class MapReportsModel
{
    [Key] public int geodata_id { get; set; } // Ingen MinLength på int

    [Required]
    [MinLength(5)]
    public string? Melding { get; set; } // MinLength fungerer kun på strenger, arrays, eller samlinger

    [Required] public string? StringKoordinaterLag { get; set; } // Ingen valideringsattributt som ikke passer
    public virtual Users? Users { get; set; }

    [NotMapped] // Legges ikke til i database
    public string? FeilMelding { get; set; }
}