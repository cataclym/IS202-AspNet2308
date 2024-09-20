using System.ComponentModel.DataAnnotations;

namespace Kartverk.Models;

public class FeilMeldingsModel
{
    [Required]
    public int[]? KoordinaterLag { get; set; }
    [Required]
    [MinLength(5)]
    [MaxLength(1024)]
    public string? Melding { get; set; }
}