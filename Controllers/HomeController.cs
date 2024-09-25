using System.Diagnostics;
using System.Text.Json;
using Kartverket.Models;
using Microsoft.AspNetCore.Mvc;

namespace Kartverket.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
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

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
    
    [HttpPost]
    public IActionResult Register(FeilMeldingsModel model)
    {
        if (ModelState.IsValid)
        {
            // Her lagrer vi GeoJSON-strukturen i databasen eller behandler den videre
            string geojsonData = model.StringKoordinaterLag;

            // Validering av GeoJSON (valgfritt, avhengig av behov)
            if (!IsValidGeoJson(geojsonData))
            {
                model.FeilMelding = "GeoJSON data is not valid.";
                return View(model);
            }

            // Lagrer til databasen (avhengig av hvordan du har satt opp lagring)
            // dbContext.FeilMeldingsModels.Add(model);
            // dbContext.SaveChanges();

            return RedirectToAction("Success");
        }

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
    public ViewResult HomePage(LoginDataModel loginData)
    {
        // Hvis registreringen er vellykket, send dataene videre til profilen
        return View("HomePage", loginData);
    }

    // GET: Viser registreringsskjemaet
    [HttpGet]
    public ViewResult HomePage()
    {
        return View();
    }
}