using System.Diagnostics;
using System.Text.Json;
using Kartverket.Models;
using Microsoft.AspNetCore.Mvc;
using Kartverket.Database;
using Kartverket.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Kartverket.Database.Models;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;

namespace Kartverket.Controllers;

public class HomeController　: Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<HomeController> _logger;
    private readonly MunicipalityService _municipalityService;
    
    // Constructor for å injisere ApplicationDbContext
    public HomeController(ApplicationDbContext context, ILogger<HomeController> logger, MunicipalityService municipalityService)
    {
        _context = context;
        _logger = logger;
        _municipalityService = municipalityService;
    }
    
    public IActionResult Index()
    {
        // Brukeren er ikke autentisert
        // Brukeren er autentisert
        Console.WriteLine(User.Identity.IsAuthenticated ? "User is authenticated" : "User is not authenticated");
        return View();
    }

    public IActionResult Privacy()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            // Brukeren er autentisert
            Console.WriteLine("User is authenticated");
        }
        else
        {
            // Brukeren er ikke autentisert
            Console.WriteLine("User is not authenticated");
        }
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [HttpPost]
    public async Task<IActionResult> RegisterMapReport(ReportViewModel model)
    {
        // Logg modellens verdi
        _logger.LogInformation("Model: {Model}", @model);

        // Sjekk om modellen er gyldig
        if (ModelState.IsValid)
        {
            _logger.LogInformation("ModelState er gyldig");
            _logger.LogInformation("GeoJSON-streng: {GeoJson}", model.GeoJsonString);

            // Konverter GeoJSON-strengen til koordinater
            string coordinatesString = model.ConvertGeoJsonStringToCoordinates();
            _logger.LogInformation("Konverterte koordinater: {Coordinates}", coordinatesString);

            ViewBag.Coordinates = coordinatesString;

            // Sjekk om koordinatene er gyldige (hvis det er nødvendig)
            if (string.IsNullOrWhiteSpace(coordinatesString))
            {
                ViewData["ErrorMessage"] = "Ugyldige koordinater fra GeoJSON-strengen.";
                _logger.LogWarning("Konverterte koordinater er ugyldige eller tomme.");
                return View("MapReport", model); // Returner til view hvis koordinatene er ugyldige
            }

            var mapLayers = GetGeoJson(model.GeoJsonString);
            
            // Validering av GeoJSON (valgfritt, avhengig av behov)
            if (mapLayers == null)
            {
                ViewData["ErrorMessage"] = "Du må markere området på kartet.";
                _logger.LogWarning("GeoJSON-data er ugyldig");
                return View("MapReport", model); // Returner til view hvis GeoJSON er ugyldig
            }
            
            MunicipalityCountyNames? municipalityInfo = await _municipalityService.GetMunicipalityFromCoordAsync(mapLayers);
            
            ViewData["MunicipalityInfo"] = municipalityInfo;
            
            _logger.LogInformation("Legger til data i databasen");
            
            
            // Forutsetter at brukeren er autentisert
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            // Konstruerer database-modellen for Reports
            var report = new Reports
            {
                UserId = int.Parse(userId), // Henter brukerens ID fra claims
                GeoJsonString = model.GeoJsonString,
                CreatedAt = DateTime.Now,
                Messages = new List<Messages>() // Oppretter en tom liste for meldinger
            };

            // Legg til meldingen til Messages-listen hvis det finnes en melding i modellen
            if (!string.IsNullOrWhiteSpace(model.Message))
            {
                report.Messages.Add(new Messages
                {
                    Message = model.Message,
                    CreatedAt = DateTime.Now
                });
            }

            // Legg til rapporten i databasen
            _context.Reports.Add(report);

            // Lagre endringer til databasen asynkront
            await _context.SaveChangesAsync();
            _logger.LogInformation("Data har blitt lagret i databasen");

            return View("Reported", model); // Eller til et annet view for å bekrefte lagringen

            return View("Reported", model); // Eller til et annet view for å bekrefte lagringen
        }

        _logger.LogWarning("ModelState er ugyldig. Returnerer til view");
        foreach (var state in ModelState)
        {
            foreach (var error in state.Value.Errors)
            {
                _logger.LogWarning($"Valideringsfeil for '{state.Key}': {error.ErrorMessage}");
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
    public ViewResult MapReport(ReportViewModel? feilMeldingsModel)
    {
        return View("MapReport", feilMeldingsModel);
    }

    [HttpGet]
    public ViewResult MapReport()
    {
        return View("MapReport");
    }
    
    
    [HttpPost]
    public async Task<IActionResult> HomePage(UsersModel usersModelModel)
    {
        // Sjekk om modellen er gyldig
        if (!ModelState.IsValid) return View(usersModelModel);
        
        try
        {
            var users = new Users
            {
                Username = usersModelModel.Username,
                Password = usersModelModel.Password,
                Email = usersModelModel.Email,
                Phone = usersModelModel.Phone,
                IsAdmin = usersModelModel.IsAdmin,
            };
            // Legger til brukerdata i databasen
            _context.Users.Add(users);

            // Lagre endringer til databasen asynkront
            await _context.SaveChangesAsync();

            // Gå til en suksess- eller bekreftelsesside (eller tilbakemelding på skjema)
            return View("HomePage", usersModelModel);
        }   
        catch (Exception ex)
        {
            // Logg feil hvis lagringen ikke fungerer
            _logger.LogError(ex, "Feil ved lagring av brukerdata");
            // Returner en feilmelding
            return View("Error");
        }
    }

    // GET: Viser registreringsskjemaet
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> HomePage(int id = 0)
    {
        // Hvis id ikke er satt, hent det fra claims
        if (id == 0)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                id = userId;
            }
            else
            {
                _logger.LogInformation("Could not retrieve user id from claims or URL.");
                return RedirectToAction("Login", "Account");
            }
        }

        _logger.LogInformation("User id retrieved: {id}", id);

        // Finn brukeren i databasen basert på UserId
        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);

        if (user == null)
        {
            _logger.LogInformation("User not found for id: {id}", id);
            return RedirectToAction("Login", "Account");
        }
        
        // Returner brukerdata til viewet
        
        // Bruk mapper funksjonen for å konvertere `Users` til `UsersModel`
        var userModel = UsersModel.FromUsers(user);
        
        // Hent rapporter for brukeren, sorter etter CreatedAt, og hent ReportId, Message og Status
        var reports = await _context.Reports
            .Where(r => r.UserId == id)
            .OrderBy(r => r.CreatedAt)
            .Include(r => r.Messages) // Inkluderer relasjonen til Messages
            .Select(r => new ReportViewModel
            {
                ReportId = r.ReportId,
                Message = r.Messages != null && r.Messages.Any() ? r.Messages.First().Message : "Ingen melding",
                Status = r.Status
            })
            .ToListAsync();

        var viewModel = new HomepageViewModel
        {
            User = userModel, 
            Reports = reports  
        };

        return View(viewModel);
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
    
    public ViewResult Login()
    {
        return View();
    }
}
