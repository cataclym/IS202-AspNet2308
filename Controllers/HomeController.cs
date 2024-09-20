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

    [HttpGet]
    public ViewResult MapMistakeRegistrationForm()
    {
        return View("RegistrationForm");
    }

    [HttpGet]
    public ViewResult LoginForm()
    {
        return View("Homepage");
    }
    
    [HttpPost]
    public ViewResult LoginForm(LoginData loginData)
    {
        return !ModelState.IsValid ? View("Index", loginData) : View("Homepage", loginData);
    }
}
