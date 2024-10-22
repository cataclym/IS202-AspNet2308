using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace Kartverket.Models;

[Table("Reports")]
public sealed class MapReportsModel
{
    [Key] public int GeodataId { get; set; } // Ingen MinLength på int

    [Required]
    [MinLength(5)]
    [MaxLength(256)]
    public string Melding { get; set; } // MinLength fungerer kun på strenger, arrays, eller samlinger

    [Required] public string StringKoordinaterLag { get; set; } // Ingen valideringsattributt som ikke passer

    // Referrer til Messages tabell
    public ICollection<MessagesModel> Messages { get; set; } = new List<MessagesModel>();
    public int UserId { get; set; }
    //Konverterer GeoJson koordinater til en streng med koordinater
    public string ConvertGeoJsonStringToCoordinates()
    {
        if (string.IsNullOrWhiteSpace(StringKoordinaterLag))
        {
            return "Ingen GeoJSON-streng tilgjengelig";
        }

        try
        {
            // Parse GeoJSON-strengen
            var geoJsonObject = JObject.Parse(StringKoordinaterLag);

            // Sjekk om dette er en 'FeatureCollection'
            var features = geoJsonObject["features"];
            if (features != null && features.Type == JTokenType.Array)
            {
                // Håndter hver 'Feature' i 'FeatureCollection'
                foreach (var feature in features)
                {
                    var geometryType = feature["geometry"]?["type"]?.ToString();
                    var coordinates = feature["geometry"]?["coordinates"];

                    if (coordinates is null) throw new Exception("Ingen koordinater tilgjengelige");
                    
                    // Hent koordinater avhengig av typen geometri
                    return geometryType switch
                    {
                        "Polygon" => ParsePolygonCoordinates(coordinates),
                        "MultiPolygon" => ParseMultiPolygonCoordinates(coordinates),
                        _ => "Geometri-typen støttes ikke eller er ukjent"
                    };
                }
            }
            else
            {
                return "Ingen 'features' funnet i GeoJSON-strengen";
            }

            return "Ingen gyldige koordinater funnet i GeoJSON";
        }
        catch (Exception ex)
        {
            return $"Feil ved parsing av GeoJSON-streng: {ex.Message}";
        }
    }
    private static string ParsePolygonCoordinates(JToken coordinates)
    {
        if (coordinates is not { Type: JTokenType.Array }) return "Ingen koordinater funnet for Polygon";
        
        var sb = new StringBuilder();
        sb.Append("Lokasjon: ");
        foreach (var ring in coordinates)
        {
            foreach (var coordPair in ring)
            {
                var lng = (double?) coordPair[0];
                var lat = (double?) coordPair[1];
                sb.Append($"[Lat: {lat}, Lng: {lng}], ");
            }
        }
        return sb.ToString().TrimEnd(',', ' ');
    }
    private static string ParseMultiPolygonCoordinates(JToken coordinates)
    {
        if (coordinates is not { Type: JTokenType.Array }) return "Ingen koordinater funnet for MultiPolygon";
        
        var sb = new StringBuilder();
        sb.Append("MultiPolygon koordinater: ");
        foreach (var polygon in coordinates)
        {
            sb.Append(ParsePolygonCoordinates(polygon)).Append(" | ");
        }
        return sb.ToString().TrimEnd(' ', '|');
    }
}