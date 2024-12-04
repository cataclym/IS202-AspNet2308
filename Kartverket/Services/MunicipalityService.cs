using System.Globalization;
using System.Text.Json;
using Kartverket.Models;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json.Linq;

namespace Kartverket.Services;

public class MunicipalityService : IMunicipalityService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<MunicipalityService> _logger;

    public MunicipalityService(HttpClient httpClient, ILogger<MunicipalityService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }
    
    // Hent informasjon om kommune, fra kommunenummeret
    public async Task<KommuneInfo?> GetMunicipalityInfoAsync(string kommuneNr)
    {
        try
        {
            // Send en GET forespørsel til kartverkets API, med kommuner endpoint
            var response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}/kommuner/{kommuneNr}");
            // Kaster hvis responsen ikke er ok
            response.EnsureSuccessStatusCode();

            // Hent json som string
            var json = await response.Content.ReadAsStringAsync();
            _logger.LogInformation($"KommuneInfo Response: {json}");
            
            // Hent JSON fra string med KommuneInfo Modellen 
            var municipalityInfo = JsonSerializer.Deserialize<KommuneInfo>(json);
            return municipalityInfo; 
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching KommuneInfo for {kommuneNr}: {ex.Message}");
            return null;
        }
    }
    public async Task<MunicipalityCountyNames?> GetMunicipalityFromCoordAsync(MapLayersModel? mapLayers)
    {
        if (mapLayers == null) return null;
        
        var geometry = mapLayers.features.First().geometry;
        var coords = geometry.coordinates;
        List<double>? geometricCoordinates = null;
        
        try
        {
            switch (geometry.type)
            {
                case "Point":
                    geometricCoordinates = coords.Cast<double>().ToList();
                    break;
                case "LineString":
                    geometricCoordinates = (coords.First() as JArray)?.ToObject<List<double>>();
                    break;
                case "Polygon":
                    geometricCoordinates = ((coords.FirstOrDefault() as JArray)?.FirstOrDefault() as JArray)?.ToObject<List<double>>();
                    break;
            }
            
            if (geometricCoordinates == null || geometricCoordinates.Count < 2)
            {
                _logger.LogError("Ugyldige geometriske koordinater.");
                return null;
            }

            // Her defineres query parametere
            var query = new Dictionary<string, string?>()
            {
                ["nord"] = geometricCoordinates[1].ToString(CultureInfo.InvariantCulture),
                ["ost"] = geometricCoordinates[0].ToString(CultureInfo.InvariantCulture),
                ["koordsys"] = "4258"
            };

            // Send en GET forespørsel til kartverkets API, med punkt endpoint
            var url = QueryHelpers.AddQueryString($"{_httpClient.BaseAddress}/punkt", query);
            _logger.LogInformation("Henter kommuneinfo fra: {Url}", url);
            var response = await _httpClient.GetAsync(url);
            
            // Kaster hvis responsen ikke er ok
            response.EnsureSuccessStatusCode();
            
            // Hent json som string
            var json = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Kommune punkt Response: {Json}", json);
            
            // Hent JSON fra string med KommuneInfo Modellen 
            return JsonSerializer.Deserialize<MunicipalityCountyNames>(json);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error fetching KommuneInfo for punkt: {ExMessage} - {ExStackTrace}", ex.Message, ex.StackTrace);
            return null;
        }
    }

}
