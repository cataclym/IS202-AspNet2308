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

    public string? FirstMessage { get; set; } // The first message

    public List<MessagesModel> Messages { get; set; } = new List<MessagesModel>();


    [Required(ErrorMessage = "Du må markere området på kartet.")]
    public required string GeoJsonString { get; set; }

    public string? Username { get; set; }

    public bool IsAdmin { get; set; } = false;

    public int? AssignedAdminId { get; set; }
    public string? AssignedAdminUsername { get; set; }

    public bool IsPinned { get; set; }

    public MunicipalityCountyNames? MunicipalityInfo { get; set; } // Add this line

    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public Status Status { get; set; }


    public string StringKoordinaterLag
    {
        get => GeoJsonString;
        set => GeoJsonString = value;
    }
}

    