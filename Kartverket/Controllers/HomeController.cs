using System.Diagnostics;
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
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;
using System.Linq;
using Kartverket.Services;

namespace Kartverket.Controllers;

public class HomeController: Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<HomeController> _logger;
    private readonly IMunicipalityService _municipalityService;
    private readonly IUserService _userService;
    
    // Constructor for å injisere ApplicationDbContext
    public HomeController(ApplicationDbContext context, ILogger<HomeController> logger, IMunicipalityService municipalityService, IUserService userService)
    {
        _context = context;
        _logger = logger;
        _municipalityService = municipalityService;
        _userService = userService;

    }
    
    public IActionResult Index()
    {
        // Brukeren er ikke autentisert
        // Brukeren er autentisert
        Console.WriteLine(User.Identity is { IsAuthenticated: true } ? "User is authenticated" : "User is not authenticated");
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
    
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> MyPage(int id = 0)
    {
        // Hent bruker-ID basert på input-id (eller annen logikk i GetUserIdAsync)
        id = await _userService.GetUserIdAsync(id);

        if (id == 0)
        {
            // Bruker-ID kunne ikke hentes, omdiriger til login
            return RedirectToAction("Login", "Account");
        }

        // Hent brukerdata fra databasen
        var user = await _userService.GetUserAsync(id);

        if (user == null)
        {
            // Bruker finnes ikke, omdiriger til login
            return RedirectToAction("Login", "Account");
        }

        // Mapper brukerdata til MyPageModel
        var model = new MyPageModel
        {
            Username = user.Username,
            Email = user.Email,
            Phone = user.Phone
        };
        
        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            // Hent brukerinformasjon hvis nødvendig
            var isAdmin = User.IsInRole("Admin");
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (isAdmin)
                return RedirectToAction("AdminDashboard", "Home");
        }
        
        // Sender modellen til viewet
        return View(model);
    }

    public async Task<IActionResult> AdminDashboard()
    {
        // Finner dagens dato ved midnatt
        DateTime today = DateTime.Today;
        
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier); // Henter bruker-ID som string
        if (!int.TryParse(userIdString, out int userId))
        {
            // Håndter feilen hvis userId ikke kan konverteres til int (kan returnere en feilmelding eller omdirigere)
            return BadRequest("Ugyldig bruker-ID.");
        }
        
        var user = await _context.Users.FindAsync(userId);
        
        var viewModel = new AdminDashboardModel
        {
            UnprocessedReportsCount = await _context.Reports
                .Where(r => r.Status == Status.Ubehandlet) // Bruk enum-verdi direkte
                .CountAsync(),
            
            ReportsTodayCount = await _context.Reports
            .Where(r => r.CreatedAt >= today) // Henter rapporter meldt inn fra dagens start
            .CountAsync(),
            
            ProcessedReportsCount = await _context.Reports
                .Where(r => r.Status == Status.Behandlet)
                .CountAsync(),

            ReportsUnderTreatmentCount = await _context.Reports
                .Where(r => r.Status == Status.Under_Behandling)
                .CountAsync(),
            
            Username = user?.Username,
            Email = user?.Email,
            Phone = user?.Phone
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
