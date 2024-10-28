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
    
    
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public Status Status { get; set; }


    public string StringKoordinaterLag
    {
        get => GeoJsonString;
        set => GeoJsonString = value;
    }
    

    public string ConvertGeoJsonStringToCoordinates()
    {
        if (string.IsNullOrWhiteSpace(GeoJsonString))
        {
            return "Ingen GeoJSON-streng tilgjengelig";
        }

        try
        {
            var geoJsonObject = JObject.Parse(GeoJsonString);
            var features = geoJsonObject["features"];
            if (features != null && features.Type == JTokenType.Array)
            {
                foreach (var feature in features)
                {
                    var geometryType = feature["geometry"]?["type"]?.ToString();
                    var coordinates = feature["geometry"]?["coordinates"];

                    if (coordinates is null) throw new Exception("Ingen koordinater tilgjengelige");

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
                var lng = (double?)coordPair[0];
                var lat = (double?)coordPair[1];
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
