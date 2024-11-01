using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Kartverket.Models;
using Newtonsoft.Json;


namespace Kartverket.Services
{
    public class GeoJsonService
    {
        private readonly ILogger<GeoJsonService> _logger;

        public GeoJsonService(ILogger<GeoJsonService> logger)
        {
            _logger = logger;
        }

        public string ConvertGeoJsonToString(string geoJsonString)
        {
            if (string.IsNullOrWhiteSpace(geoJsonString))
            {
                return "No GeoJSON data available";
            }

            try
            {
                var geoJsonObject = JObject.Parse(geoJsonString);
                string type = geoJsonObject["type"]?.ToString();

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
                                if (!string.IsNullOrEmpty(featureDescription))
                                {
                                    descriptions.Add(featureDescription);
                                }
                            }
                            return string.Join("\n", descriptions);
                        }
                        break;

                    case "Feature":
                        return ProcessFeature(geoJsonObject);

                    default:
                        return "Unsupported GeoJSON type at root";
                }
            }
            catch (JsonReaderException ex)
            {
                _logger.LogError("Invalid GeoJSON format: {message}", ex.Message);
                return "Invalid GeoJSON data";
            }

            return "Unknown GeoJSON format";
        }

        private string ProcessFeature(JObject feature)
        {
            if (feature == null)
            {
                return null;
            }

            var geometry = feature["geometry"] as JObject;
            if (geometry != null)
            {
                string geomType = geometry["type"]?.ToString();
                var coordinates = geometry["coordinates"] as JArray;

                switch (geomType)
                {
                    case "Point":
                        if (coordinates != null && coordinates.Count == 2)
                        {
                            double longitude = (double)coordinates[0];
                            double latitude = (double)coordinates[1];
                            return $"Point at Latitude: {latitude}, Longitude: {longitude}";
                        }
                        break;

                    case "LineString":
                        if (coordinates != null)
                        {
                            var points = coordinates.Select(coord => $"({coord[1]}, {coord[0]})");
                            return "Line through points: " + string.Join(" -> ", points);
                        }
                        break;

                    case "Polygon":
                        if (coordinates != null)
                        {
                            var rings = coordinates.First() as JArray;
                            if (rings != null)
                            {
                                var points = rings.Select(coord => $"({coord[1]}, {coord[0]})");
                                return "Polygon with vertices: " + string.Join(", ", points);
                            }
                        }
                        break;

                    default:
                        return "Unsupported geometry type in feature";
                }
            }

            return "Unknown geometry in feature";
        }
        public MapLayersModel? GetGeoJson(string geojson)
        {   
            try
            {
                var obj = JsonConvert.DeserializeObject<MapLayersModel>(geojson);

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
}
