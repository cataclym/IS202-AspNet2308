using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kartverket.Models;

[Table("Reports")]
public class MapReportsModel
{
    [Key] public int geodata_id { get; set; } // Ingen MinLength på int

    [Required]
    [MinLength(5)]
    public string? Melding { get; set; } // MinLength fungerer kun på strenger, arrays, eller samlinger

    [Required] public string? StringKoordinaterLag { get; set; } // Ingen valideringsattributt som ikke passer

    // Referrer til Users tabell
    public virtual Users? Users { get; set; }

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

                    // Hent koordinater avhengig av typen geometri
                    switch (geometryType)
                    {
                        case "Polygon":
                            return ParsePolygonCoordinates(coordinates);
                    
                        case "MultiPolygon":
                            return ParseMultiPolygonCoordinates(coordinates);

                        default:
                            return "Geometri-typen støttes ikke eller er ukjent";
                    }
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
    private string ParsePolygonCoordinates(JToken coordinates)
    {
        if (coordinates != null && coordinates.Type == JTokenType.Array)
        {
            var sb = new StringBuilder();
            sb.Append("Lokasjon: ");
            foreach (var ring in coordinates)
            {
                foreach (var coordPair in ring)
                {
                    double lng = (double)coordPair[0];
                    double lat = (double)coordPair[1];
                    sb.Append($"[Lat: {lat}, Lng: {lng}], ");
                }
            }
            return sb.ToString().TrimEnd(',', ' ');
        }
        return "Ingen koordinater funnet for Polygon";
    }
    private string ParseMultiPolygonCoordinates(JToken coordinates)
    {
        if (coordinates != null && coordinates.Type == JTokenType.Array)
        {
            var sb = new StringBuilder();
            sb.Append("MultiPolygon koordinater: ");
            foreach (var polygon in coordinates)
            {
                sb.Append(ParsePolygonCoordinates(polygon)).Append(" | ");
            }
            return sb.ToString().TrimEnd(' ', '|');
        }
        return "Ingen koordinater funnet for MultiPolygon";
    }
}