using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace Kartverket.Database.Models;

[Table("Reports")]
public sealed class Reports
{
    [Key] public int ReportId { get; set; } // Ingen MinLength på int
    
    // Fremmednøkkel til bruker
    public int UserId { get; set; }
    public Users User { get; set; }
    
    [MaxLength(2000)]
    public string GeoJsonString { get; set; }
    public Status Status { get; set; } = Status.Ubehandlet;
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    // Referrer til Messages tabell
    public ICollection<Messages> Messages { get; set; } = new List<Messages>();
    public ICollection<PinnedReport> PinnedReports { get; set; }
    
    [ForeignKey("AssignedAdmin")]
    public int? AssignedAdminId { get; set; }

    // Navigation property to the assigned admin
    public Users AssignedAdmin { get; set; }

    [ForeignKey("MunicipalityId")]
    public int? MunicipalityId { get; set; }
    public Municipality? Municipality { get; set; }
}

public enum Status
{
    Ubehandlet,
    Under_Behandling,
    Behandlet
}