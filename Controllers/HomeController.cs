using System.Diagnostics;
using System.Text.Json;
using Kartverket.Models;
using Microsoft.AspNetCore.Mvc;
using Kartverket.Data;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;


namespace Kartverket.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<HomeController> _logger;

    public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View("Register");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [HttpPost]
    public async Task<IActionResult> Register(FeilMeldingsModel model)
    {

        // Logg modellens verdi
        _logger.LogInformation($"Model: {@model}");


        // Sjekk om modellen er gyldig
        if (ModelState.IsValid)
        {
            _logger.LogInformation("ModelState er gyldig.");

            // Her henter vi GeoJSON-strukturen fra modellen
            string geojsonData = model.StringKoordinaterLag;

            // Validering av GeoJSON (valgfritt, avhengig av behov)
            if (!IsValidGeoJson(geojsonData))
            {
                model.FeilMelding = "GeoJSON data is not valid.";
                _logger.LogWarning("GeoJSON-data er ugyldig.");
                return View(model); // Returner til view hvis GeoJSON er ugyldig
            }

            // Lagrer til databasen
            // Bruker _context som er injisert i controlleren
            _logger.LogInformation("Legger til data i databasen.");
            _context.FeilMeldinger.Add(model);

            // Lagre endringer til databasen asynkront
            await _context.SaveChangesAsync();
            _logger.LogInformation("Data har blitt lagret i databasen.");

            return View("register", model); // Eller til et annet view for å bekrefte lagringen
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
        return View(model);
    }

// Valideringsmetode for GeoJSON (valgfritt)
    private bool IsValidGeoJson(string geojson)
    {
        try
        {
            var obj = JsonSerializer.Deserialize<JsonDocument>(geojson);
            return obj?.RootElement.GetProperty("type").GetString() == "FeatureCollection";
        }
        catch
        {
            return false;
        }
    }


    [HttpPost]
    public ViewResult RegistrationForm(FeilMeldingsModel? feilMeldingsModel)
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
    public ViewResult LoginForm(LoginDataModel loginDataModel)
    {
        return !ModelState.IsValid ? View("Index", loginDataModel) : View("Homepage", loginDataModel);
    }


    [HttpPost]
    public ViewResult RegistrationPage(LoginDataModel loginData)
    {
        // Hvis registreringen er vellykket, send dataene videre til profilen
        return View("RegistrationPage", loginData);
    }

    // GET: Viser registreringsskjemaet
    [HttpGet]
    public ViewResult RegistrationPage()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> HomePage(LoginDataModel loginData)
    {
        // Sjekk om modellen er gyldig
        if (ModelState.IsValid)
        {
            try
            {
                // Legger til brukerdata i databasen
                _context.LoginData.Add(loginData);

                // Lagre endringer til databasen asynkront
                await _context.SaveChangesAsync();

                // Gå til en suksess- eller bekreftelsesside (eller tilbakemelding på skjema)
                return RedirectToAction("RegistrationForm"); // Eller returner View("HomePage") for å vise data
            }
            catch (Exception ex)
            {
                // Logg feil hvis lagringen ikke fungerer
                _logger.LogError(ex, "Feil ved lagring av brukerdata.");
                // Returner en feilmelding
                return View("Error");
            }
        }

        return View(loginData);
    }

    // GET: Viser registreringsskjemaet
    [HttpGet]
    public ViewResult HomePage()
    {
        return View();
    }
}
