using Kartverket.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kartverket.Services;

public class GeoJsonService
{
    private readonly ILogger<GeoJsonService> _logger;

    public GeoJsonService(ILogger<GeoJsonService> logger)
    {
        _logger = logger;
    }

    public string ConvertGeoJsonToString(string geoJsonString)
    {
        if (string.IsNullOrWhiteSpace(geoJsonString)) return "No GeoJSON data available";

        try
        {
            var geoJsonObject = JObject.Parse(geoJsonString);
            var type = geoJsonObject["type"]?.Value<string>();

            switch (type)
            {
                case "FeatureCollection":
                    var features = geoJsonObject["features"] as JArray;
                    if (features != null)
                    {
                        var descriptions = new List<string>();
                        foreach (var feature in features)
                        {
                            var featureDescription = ProcessFeature(feature as JObject);
                            if (!string.IsNullOrEmpty(featureDescription)) descriptions.Add(featureDescription);
                        }

                        return string.Join("\n", descriptions);
                    }

                    break;

                case "Feature":
                {
                    var featureDescription = ProcessFeature(geoJsonObject);
                    if (featureDescription != null)
                    {
                        return featureDescription;
                    }
                    break;
                }

                default:
                    return "Unsupported GeoJSON type at root";
            }
        }
        catch (JsonReaderException ex)
        {
            _logger.LogError("Invalid GeoJSON format: {Message}", ex.Message);
            return "Invalid GeoJSON data";
        }

        return "Unknown GeoJSON format";
    }

    private string? ProcessFeature(JObject? feature)
    {
        if (feature == null) return null;

        var geometry = feature.ToObject<Feature>()?.geometry;

        if (geometry == null) return "Unknown geometry in feature";

        var geomType = geometry.type;
        var coordinates = geometry.coordinates;

        switch (geomType)
        {
            case "Point":
            {
                var longitude = coordinates[0];
                var latitude = coordinates[1];
                return $"Point at Latitude: {latitude}, Longitude: {longitude}";
            }
            
            case "LineString":
            {
                var points = coordinates.Select(coord => coord is JArray coords ? $"({coords[1]}, {coords[0]})" : null);
                return "Line through points: " + string.Join(" -> ", points);
            }
            
            case "Polygon":
                if (coordinates.First() is JArray rings)
                {
                    var polygonPoints = rings.Select(coord => $"({coord[1]}, {coord[0]})");
                    return "Polygon with vertices: " + string.Join(", ", polygonPoints);
                }
                break;
            
            default:
                return "Unsupported geometry type in feature";
        }

        return "Unknown geometry in feature";
    }
    
    public MapLayersModel? GetGeoJson(string geoJson)
    {
        try
        {
            var obj = JsonConvert.DeserializeObject<MapLayersModel>(geoJson);

            if (obj?.features?.Count > 0) return obj;
        }
        catch (JsonException ex)
        {
            _logger.LogError("Deserialization failed: {message}", ex.Message);
            return null;
        }

        return null;
    }
}