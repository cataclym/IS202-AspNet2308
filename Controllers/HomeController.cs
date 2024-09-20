using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Kartverk.Models;

namespace Kartverk.Controllers;

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
    public ViewResult RegistrationForm(FeilMeldingsModel feilMeldingsModel)
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
}
