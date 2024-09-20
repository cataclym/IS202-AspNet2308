using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Kartverk.mvc.Models;

namespace Kartverk.mvc.Controllers;

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
        return View();
    }

    [HttpGet]
    public ViewResult LoginForm()
    {
        return View("Homepage");
    }
    
    [HttpPost]
    public IActionResult LoginForm(LoginData loginData)
    {
        return !ModelState.IsValid ? View() : View("Homepage", loginData);
    }
}
