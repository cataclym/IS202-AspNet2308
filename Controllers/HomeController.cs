using System.Diagnostics;
using System.Text.Json;
using Kartverket.Models;
using Microsoft.AspNetCore.Mvc;
using Kartverket.Data;
using Kartverket.Services;

namespace Kartverket.Controllers;

public class HomeController(
    ApplicationDbContext context,
    ILogger<HomeController> logger,
    MunicipalityService municipalityService)
    : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [HttpPost]
    public async Task<IActionResult> RegisterMapReport(MapReportsModel model)
    {
        // Logg modellens verdi
        logger.LogInformation("Model: {Model}", @model);

        // Sjekk om modellen er gyldig
        if (ModelState.IsValid)
        {
            logger.LogInformation("ModelState er gyldig");

            var mapLayers = GetGeoJson(model.StringKoordinaterLag);
            
            // Validering av GeoJSON (valgfritt, avhengig av behov)
            if (mapLayers == null)
            {
                ViewData["ErrorMessage"] = "Du må markere området på kartet.";
                logger.LogWarning("GeoJSON-data er ugyldig");
                return View("MapReport", model); // Returner til view hvis GeoJSON er ugyldig
            }
            
            MunicipalityCountyNames? municipalityInfo = await municipalityService.GetMunicipalityFromCoordAsync(mapLayers);
            
            ViewData["MunicipalityInfo"] = municipalityInfo;
            
            // Lagrer til databasen
            // Bruker _context som er injisert i controlleren
            logger.LogInformation("Legger til data i databasen");
            context.MapReports.Add(model);

            // Lagre endringer til databasen asynkront
            await context.SaveChangesAsync();
            logger.LogInformation("Data har blitt lagret i databasen");

            return View("Reported", model); // Eller til et annet view for å bekrefte lagringen
        }

        logger.LogWarning("ModelState er ugyldig. Returnerer til view");
        foreach (var state in ModelState)
        {
            foreach (var error in state.Value.Errors)
            {
                logger.LogWarning($"Valideringsfeil for '{state.Key}': {error.ErrorMessage}");
            }
        }

        // Returner samme view med valideringsfeil hvis modellen ikke er gyldig
        return View("MapReport", model);
    }

    // Valideringsmetode for GeoJSON (valgfritt)
    private static MapLayersModel? GetGeoJson(string geojson)
    {   
        try
        {
            var obj = JsonSerializer.Deserialize<MapLayersModel>(geojson);

            if (obj?.features.Count > 0) return obj;
        }
        // Hvis Deserialize ikke fullførte, returner null
        catch
        {
            return null;
        }

        return null;
    }
    
    [HttpPost]
    public ViewResult MapReport(MapReportsModel? feilMeldingsModel)
    {
        return View("MapReport", feilMeldingsModel);
    }

    [HttpGet]
    public ViewResult MapReport()
    {
        return View("MapReport");
    }
    
    [HttpGet]
    public ViewResult LoginForm()
    {
        return View("Homepage");
    }

    [HttpPost]
    public async Task<ViewResult> LoginForm(Users usersModel)
    {
        if (!ModelState.IsValid) return View("Index", usersModel);

//        var user = await _context.Users.FindAsync(usersModel.UserName, usersModel.Password);
        
        return View("Homepage", usersModel);
    }


    [HttpPost]
    public ViewResult UserRegistration(Users usersModel)
    {
        // Hvis registreringen er vellykket, send dataene videre til profilen
        return View("UserRegistration", usersModel);
    }

    // GET: Viser registreringsskjemaet
    [HttpGet]
    public ViewResult UserRegistration()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> HomePage(Users usersModel)
    {
        // Sjekk om modellen er gyldig
        if (ModelState.IsValid)
        {
            try
            {
                // Legger til brukerdata i databasen
                context.Users.Add(usersModel);

                // Lagre endringer til databasen asynkront
                await context.SaveChangesAsync();

                // Gå til en suksess- eller bekreftelsesside (eller tilbakemelding på skjema)
                return View("HomePage", usersModel);
            }   
            catch (Exception ex)
            {
                // Logg feil hvis lagringen ikke fungerer
                logger.LogError(ex, "Feil ved lagring av brukerdata");
                // Returner en feilmelding
                return View("Error");
            }
        }

        return View(usersModel);
    }

    // GET: Viser registreringsskjemaet
    [HttpGet]
    public ViewResult HomePage()
    {
        return View();
    }


    [HttpGet]
    public ViewResult About()
    {
        return View();
    }

    public ViewResult Help()
    {
        return View();
    }

}
