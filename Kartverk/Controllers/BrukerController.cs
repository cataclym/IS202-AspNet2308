using Microsoft.AspNetCore.Mvc;

namespace Kartverk.mvc.Controllers;

public class BrukerController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
}