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

namespace Kartverket.Controllers;

public class HomeController　: Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<HomeController> _logger;
    private readonly MunicipalityService _municipalityService;
    
    // Constructor for å injisere ApplicationDbContext
    public HomeController(ApplicationDbContext context, ILogger<HomeController> logger, MunicipalityService municipalityService)
    {
        _context = context;
        _logger = logger;
        _municipalityService = municipalityService;
    }
    
    public IActionResult Index()
    {
        // Brukeren er ikke autentisert
        // Brukeren er autentisert
        Console.WriteLine(User.Identity.IsAuthenticated ? "User is authenticated" : "User is not authenticated");
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
    
    // GET: Viser registreringsskjemaet
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> HomePage(int id = 0)
{
    ViewData["Title"] = "Min Side"; // Sett tittel

    id = await GetUserIdAsync(id);
    if (id == 0)
    {
        return RedirectToAction("Login", "Account");
    }

    var user = await GetUserAsync(id);
    if (user == null)
    {
        return RedirectToAction("Login", "Account");
    }

    // Hent pinnede rapporter for brukeren
    var pinnedReportIds = await GetPinnedReportsAsync(id);

    var reports = await GetReportsAsync(user, pinnedReportIds);

    var viewModel = new HomePageModel
    {
        Reports = reports,
        User = MapUserToViewModel(user)
    };

    return View(viewModel);
}

private async Task<int> GetUserIdAsync(int id)
{
    if (id != 0) return id;

    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (int.TryParse(userIdClaim, out int userId))
    {
        _logger.LogInformation("User id retrieved: {userId}", userId);
        return userId;
    }

    _logger.LogInformation("Could not retrieve user id from claims or URL.");
    return 0;
}

private async Task<Users> GetUserAsync(int id)
{
    var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
    if (user == null)
    {
        _logger.LogInformation("User not found for id: {id}", id);
    }
    return user;
}

private async Task<List<int>> GetPinnedReportsAsync(int userId)
{
    return await _context.PinnedReports
        .Where(pr => pr.UserID == userId)
        .Select(pr => pr.ReportID)
        .ToListAsync();
}

private async Task<List<ReportViewModel>> GetReportsAsync(Users user, List<int> pinnedReportIds)
{
    if (user == null)
    {
        _logger.LogError("User is null in GetReportsAsync.");
        throw new ArgumentNullException(nameof(user));
    }

    if (pinnedReportIds == null)
    {
        _logger.LogWarning("pinnedReportIds is null. Initializing to empty list.");
        pinnedReportIds = new List<int>();
    }

    try
    {
        IQueryable<Reports> query;

        if (user.IsAdmin)
        {
            query = _context.Reports
                .AsNoTracking()
                .OrderBy(r => r.CreatedAt);
        }
        else
        {
            query = _context.Reports
                .AsNoTracking()
                .Where(r => r.UserId == user.UserId)
                .OrderBy(r => r.CreatedAt);
        }

        var pinnedReportIdsSet = new HashSet<int>(pinnedReportIds);

        return await query
            .Select(r => new ReportViewModel
            {
                ReportId = r.ReportId,
                FirstMessage = r.Messages
                    .OrderBy(m => m.CreatedAt)
                    .Select(m => m.Message)
                    .FirstOrDefault() ?? "No message",
                Status = r.Status,
                CreatedAt = r.CreatedAt,
                Username = r.User.Username,
                IsPinned = pinnedReportIdsSet.Contains(r.ReportId) // Correct casing and optimized lookup
            })
            .OrderByDescending(r => r.IsPinned) // Pinned reports first
            .ThenBy(r => r.CreatedAt) // Then by creation date
            .ToListAsync();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "An error occurred while fetching reports for user ID {UserId}.", user.UserId);
        throw; // Re-throw or handle as appropriate
    }
}

private UserRegistrationModel MapUserToViewModel(Users user)
{
    return new UserRegistrationModel
    {
        Username = user.Username,
        Email = user.Email,
        Phone = user.Phone,
        IsAdmin = user.IsAdmin
    };
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
