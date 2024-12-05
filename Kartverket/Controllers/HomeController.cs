using System.Diagnostics;
using Kartverket.Models;
using Microsoft.AspNetCore.Mvc;
using Kartverket.Database;
using Kartverket.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Kartverket.Database.Models;

namespace Kartverket.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IUserService _userService;
    
    // Constructor for å injisere ApplicationDbContext
    public HomeController(ApplicationDbContext context, IUserService userService)
    {
        _context = context;
        _userService = userService;
    }
    
    public IActionResult Index()
    {
        // Logg om bruker er autentisert
        LogAuthentication();
        return View();
    }

    public IActionResult Privacy()
    {
        // Logg om bruker er autentisert
        LogAuthentication();
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
        // Hent bruker-ID basert på input-id
        id = _userService.GetUserId(id);

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
            if (isAdmin)
                return RedirectToAction("AdminDashboard", "Home");
        }
        
        // Sender modellen til viewet
        return View(model);
    }

    
    [Authorize]
    [HttpGet]
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

    private void LogAuthentication()
    {
        Console.WriteLine(User.Identity is { IsAuthenticated: true } ? "User is authenticated" : "User is not authenticated");
    }
}
