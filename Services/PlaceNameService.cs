namespace Kartverket.Services;

public class PlaceNameService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PlaceNameService> _logger;
    private readonly string _baseUrl;

    PlaceNameService(ILogger<PlaceNameService> logger)
    {
        _httpClient = new HttpClient();
        _logger = logger;
        _baseUrl = "https://api.kartverket.no/kommuneinfo/v1";
    }

    // public async Task<PlaceNameResponse> GetPlaceNameAsync(string search)
    // {
    //     try
    //     {
    //         var response = await _httpClient.GetAsync($"{_baseUrl}/navn?sok={search}");
    //     }
    //     catch (Exception e)
    //     {
    //         Console.WriteLine(e);
    //         throw;
    //     }
    // }
}

