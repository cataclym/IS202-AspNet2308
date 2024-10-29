using System.ComponentModel.DataAnnotations;
using System.Text;
using Kartverket.Database.Models;
using Newtonsoft.Json.Linq;

namespace Kartverket.Models;

public class ReportViewModel
{
    public int ReportId { get; set; }
    public int UserId { get; set; }
    public string? Coordinates { get; set; }

    

    [Required]
    [MinLength(5, ErrorMessage = "Meldingen må være minst 5 tegn lang.")]
    [MaxLength(256)]
    public string Message { get; set; }

    [Required(ErrorMessage = "Du må markere området på kartet.")] 
    public string GeoJsonString { get; set; }
    public string? Username { get; set; }
    
    
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public Status Status { get; set; }


    public string StringKoordinaterLag
    {
        get => GeoJsonString;
        set => GeoJsonString = value;
    }
}
