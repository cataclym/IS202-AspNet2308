using Microsoft.AspNetCore.Mvc;

namespace Kartverket.Controllers;

public class BrukerController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
}