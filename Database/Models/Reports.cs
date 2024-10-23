using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kartverket.Database.Models;

[Table("Reports")]
public sealed class Reports
{
    [Key] public int ReportId { get; set; } // Ingen MinLength på int
    [MaxLength(2000)]
    public string GeoJsonString { get; set; }
    public Status Status { get; set; } = Status.Ubehandlet;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? ResolvedAt { get; set; }
    // Referrer til Messages tabell
    public ICollection<Messages> Messages { get; set; } = new List<Messages>();
}

public enum Status
{
    Ubehandlet,
    Under_Behandling,
    Ferdig_Behandlet
}