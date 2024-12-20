namespace Kartverket.Models;

public class Crs
{
    public string? Type { get; set; }
    public CrsProperties? Properties { get; set; }
}

public class CrsProperties
{
    public string? Name { get; set; }
}

public class Avgrensningsboks
{
    public string? Type { get; set; }
    public List<List<List<double>>>? Coordinates { get; set; }
    public Crs? Crs { get; set; }
}

public class PunktIOmrade
{
    public string? Type { get; set; }
    public List<double>? Coordinates { get; set; }
    public Crs? Crs { get; set; }
}

public class GyldigeNavn
{
    public string? Navn { get; set; }
    public int? Prioritet { get; set; }
    public string? Sprak { get; set; }
}

public class KommuneInfo
{
    public string? Fylkesnavn { get; set; }
    public string? Fylkesnummer { get; set; }
    public string? Kommunenavn { get; set; }
    public string? KommunenavnNorsk { get; set; }
    public string? Kommunenummer { get; set; }
    public bool SamiskForvaltningsomrade { get; set; }
    public required Avgrensningsboks Avgrensningsboks { get; set; }
    public required PunktIOmrade PunktIOmrade { get; set; }
    public required List<GyldigeNavn> GyldigeNavn { get; set; }
}

public class MunicipalityCountyNames
{
    public required string fylkesnavn { get; set; }
    public required string fylkesnummer { get; set; }
    public required string kommunenavn { get; set; }
    public required string kommunenummer { get; set; }
}