using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Kartverket.Models;

public class FeilMeldingsModel
{
    [Required(ErrorMessage = "Melding is required.")]
    [MinLength(5)]
    [MaxLength(1024)]
    [DisplayName("Beskrivelse av utbedring")]
    public string? Melding { get; set; }
    public string? StringKoordinaterLag { get; set; }
    public string? FeilMelding { get; set; }
}


