using Kartverket.Models;

namespace Kartverket.Services;

public interface IMunicipalityService
{
    Task<KommuneInfo?> GetMunicipalityInfoAsync(string kommuneNr);
    Task<MunicipalityCountyNames?> GetMunicipalityFromCoordAsync(MapLayersModel? mapLayers);
}