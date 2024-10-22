using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Kartverket.Database.Models;

[Table("Reports")]
public sealed class Reports
{
    [Key] public int ReportId { get; set; } // Ingen MinLength på int
    public string GeoJsonString { get; set; }
    // Referrer til Messages tabell
    public ICollection<Messages> Messages { get; set; } = new List<Messages>();
    public int UserId { get; set; }
}