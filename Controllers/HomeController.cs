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
    public ActionResult Register(FeilMeldingsModel feilMeldingsModel)
    {
        var koordinaterLag = JsonSerializer.Deserialize<KoordinaterLag>(feilMeldingsModel.StringKoordinaterLag!, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (koordinaterLag?.points?.Count < 1 && koordinaterLag?.lines?.Count < 1)
        {
            feilMeldingsModel.FeilMelding = "Du må markere området på kartet";
            return View("RegistrationForm", feilMeldingsModel);
        }

        feilMeldingsModel.KoordinaterLag = koordinaterLag;
        feilMeldingsModel.StringKoordinaterLag = null;

        return View("Register", feilMeldingsModel);
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
    public ViewResult Overview(LoginDataModel loginData)
    {
        // Hvis registreringen er vellykket, send dataene videre til profilen
        return View("HomePage", loginData);
    }

    // GET: Viser registreringsskjemaet
    [HttpGet]
    public ViewResult Overview()
    {
        return View();
    }
}