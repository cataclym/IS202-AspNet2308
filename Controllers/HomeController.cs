using System.Diagnostics;
using System.Text.Json;
using Kartverket.Models;
using Microsoft.AspNetCore.Mvc;
using Kartverket.Data;
using Kartverket.Services;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Kartverket.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<HomeController> _logger;
    private readonly MunicipalityService _municipalityService;

    public HomeController(ApplicationDbContext context, ILogger<HomeController> logger, MunicipalityService municipalityService)
    {
        _context = context;
        _logger = logger;
        _municipalityService = municipalityService;
    }

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
    public async Task<IActionResult> Register(MapReportsModel model)
    {
        // Logg modellens verdi
        _logger.LogInformation($"Model: {@model}");

        // Sjekk om modellen er gyldig
        if (ModelState.IsValid)
        {
            _logger.LogInformation("ModelState er gyldig.");

            var mapLayers = GetGeoJson(model.StringKoordinaterLag);
            
            // Validering av GeoJSON (valgfritt, avhengig av behov)
            if (mapLayers == null)
            {
                ViewData["ErrorMessage"] = "Du må markere området på kartet.";
                _logger.LogWarning("GeoJSON-data er ugyldig.");
                return View("RegistrationForm", model); // Returner til view hvis GeoJSON er ugyldig
            }
            
            MunicipalityCountyNames? municipalityInfo = await _municipalityService.GetMunicipalityFromCoordAsync(mapLayers);
            
            ViewData["MunicipalityInfo"] = municipalityInfo;
            
            // Lagrer til databasen
            // Bruker _context som er injisert i controlleren
            _logger.LogInformation("Legger til data i databasen.");
            _context.MapReports.Add(model);

            // Lagre endringer til databasen asynkront
            await _context.SaveChangesAsync();
            _logger.LogInformation("Data har blitt lagret i databasen.");

            return View("Register", model); // Eller til et annet view for å bekrefte lagringen
        }

        _logger.LogWarning("ModelState er ugyldig. Returnerer til view.");
        foreach (var state in ModelState)
        {
            foreach (var error in state.Value.Errors)
            {
                _logger.LogWarning($"Valideringsfeil for '{state.Key}': {error.ErrorMessage}");
            }
        }

        // Returner samme view med valideringsfeil hvis modellen ikke er gyldig
        return View("RegistrationForm", model);
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
    public ViewResult RegistrationForm(MapReportsModel? feilMeldingsModel)
    {
        return View("RegistrationForm", feilMeldingsModel);
    }

    [HttpGet]
    public ViewResult RegistrationForm()
    {
        return View("RegistrationForm");
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
    public ViewResult RegistrationPage(Users usersModel)
    {
        // Hvis registreringen er vellykket, send dataene videre til profilen
        return View("RegistrationPage", usersModel);
    }

    // GET: Viser registreringsskjemaet
    [HttpGet]
    public ViewResult RegistrationPage()
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
                _context.Users.Add(usersModel);

                // Lagre endringer til databasen asynkront
                await _context.SaveChangesAsync();

                // Gå til en suksess- eller bekreftelsesside (eller tilbakemelding på skjema)
                return View("HomePage", usersModel);
            }   
            catch (Exception ex)
            {
                // Logg feil hvis lagringen ikke fungerer
                _logger.LogError(ex, "Feil ved lagring av brukerdata.");
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
