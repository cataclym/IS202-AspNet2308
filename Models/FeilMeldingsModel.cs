using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Kartverk.Models;

public class FeilMeldingsModel
{
    [Required]
    [MinLength(5)]
    [MaxLength(1024)] 
    [DisplayName("Beskrivelse av utbedring")]
    public string? Melding { get; set; }
    public string? StringKoordinaterLag { get; set; }
    public KoordinatorLag? KoordinaterLag { get; set; }
}

public class KoordinatorLag
{   
        public List<List<List<LatLngs>>> points { get; set; }
        public List<object> lines { get; set; }

}

public class LatLngs
{
    public double lat  { get; set; }
    public double lng  { get; set; }
}