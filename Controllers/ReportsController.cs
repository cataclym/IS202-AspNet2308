using System.Security.Claims;
using Kartverket.Database;
using Kartverket.Database.Models;
using Kartverket.Models;
using Kartverket.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kartverket.Controllers;

[Authorize]
public class ReportsController : BaseController
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ReportsController> _logger;
    private readonly MunicipalityService _municipalityService;
    private readonly GeoJsonService _geoJsonService;

    public ReportsController(
        ApplicationDbContext context,
        ILogger<ReportsController> logger,
        MunicipalityService municipalityService,
        GeoJsonService geoJsonService)
    {
        _context = context;
        _logger = logger;
        _municipalityService = municipalityService;
        _geoJsonService = geoJsonService;
    }
    
    [HttpGet]
    [HttpPost]
    public ViewResult MapReport(ReportViewModel? feilMeldingsModel)
    {
        return View("MapReport", feilMeldingsModel);
    }

    [HttpGet]
    public ViewResult MapReport()
    {
        return View("MapReport");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegisterMapReport(ReportViewModel model)
    {
        // Sjekk om modellen er gyldig
        if (!ModelState.IsValid)
        {
            _logger.LogInformation("ModelState er ugyldig");
            _logger.LogInformation("GeoJSON-streng: {GeoJson}", model.GeoJsonString);
        }
        
        _logger.LogInformation("Prosseserer en gyldig mapreport");
        var geoJsonResult = _geoJsonService.ConvertGeoJsonToString(model.GeoJsonString);
        
        // Sjekk om koordinatene er gyldige (hvis det er nødvendig)
        if (string.IsNullOrWhiteSpace(geoJsonResult))
        {
            ViewData["ErrorMessage"] = "Ugyldige koordinater fra GeoJSON-strengen.";
            _logger.LogWarning("Konverterte koordinater er ugyldige eller tomme.");
            return View("MapReport", model); // Returner til view hvis koordinatene er ugyldige
        }
        
        var mapLayers = _geoJsonService.GetGeoJson(model.GeoJsonString);

        // Validering av GeoJSON (valgfritt, avhengig av behov)
        if (mapLayers == null)
        {
            ViewData["ErrorMessage"] = "Du må markere området på kartet.";
            _logger.LogWarning("GeoJSON-data er ugyldig");
            return View("MapReport", model); // Returner til view hvis GeoJSON er ugyldig
        }

        var municipalityInfo = await _municipalityService.GetMunicipalityFromCoordAsync(mapLayers);
        var userId = GetUserId();
        
        if (userId == null)
        {
            _logger.LogWarning("User ID could not be retrieved.");
            return Unauthorized();
        }

        // Konstruerer database-modellen for Reports
        var report = new Reports
        {
            UserId = userId.Value,
            GeoJsonString = model.GeoJsonString,
            CreatedAt = DateTime.Now,
            Messages = new List<Messages>() // Oppretter en tom liste for meldinger
        };

        // Legg til meldingen til Messages-listen hvis det finnes en melding i modellen
        if (!string.IsNullOrWhiteSpace(model.FirstMessage))
        {
            report.Messages.Add(new Messages
            {
                Message = model.FirstMessage,
                CreatedAt = DateTime.Now,
                UserId = userId.Value
            });
        }

        // Legg til rapporten i databasen
        _context.Reports.Add(report);
        // Lagre endringer til databasen asynkront
        await _context.SaveChangesAsync();
        _logger.LogInformation("Data har blitt lagret i databasen");
        
        var viewModel = new ReportViewModel
        {
            ReportId = report.ReportId,
            Coordinates = geoJsonResult,
            GeoJsonString = report.GeoJsonString,
            CreatedAt = report.CreatedAt,
            FirstMessage = model.FirstMessage,
            MunicipalityInfo = municipalityInfo
        };

        return View("Reported", viewModel); // Eller til et annet view for å bekrefte lagringen
    }
    
    private async Task SaveReportAsync(ReportViewModel model, int userId)
    {
        var report = new Reports
        {
            UserId = userId,
            GeoJsonString = model.GeoJsonString,
            CreatedAt = DateTime.Now,
            Messages = !string.IsNullOrWhiteSpace(model.FirstMessage)
                ? new List<Messages>
                    { new Messages { Message = model.FirstMessage, CreatedAt = DateTime.Now, UserId = userId } }
                : new List<Messages>()
        };
        _context.Reports.Add(report);
        await _context.SaveChangesAsync();
    }

    public async Task<IActionResult> ReportView(int id)
    {
        _logger.LogInformation("Loading report with ID: {id}", id);

        // Fetch the report by ReportId, including any associated messages
        var report = await _context.Reports
            .Include(r => r.Messages)
            .ThenInclude(m => m.User)
            .Include(r => r.User) // Hent brukerdata for selve rapporten
            .FirstOrDefaultAsync(r => r.ReportId == id);

        // Handle the case where the report is not found
        if (report == null)
        {
            _logger.LogWarning("Report with ID {id} not found", id);
            return NotFound(); // Alternatively, redirect to a "not found" page or error view
        }
        
        var mapLayers = _geoJsonService.GetGeoJson(report.GeoJsonString);
        var municipalityInfo = await _municipalityService.GetMunicipalityFromCoordAsync(mapLayers);

        // Parse the GeoJsonString into a readable format
        var normalString = _geoJsonService.ConvertGeoJsonToString(report.GeoJsonString);

        // Sjekker om brukeren er admin
        bool isAdmin = int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId)
                       && (await _context.Users.FindAsync(userId))?.IsAdmin == true;


        // Populate a view model with the necessary data
        var viewModel = new ReportViewModel
        {
            ReportId = report.ReportId,
            Coordinates = normalString,
            GeoJsonString = report.GeoJsonString,
            CreatedAt = report.CreatedAt,
            FirstMessage = report.Messages.FirstOrDefault()?.Message ?? "No message available",
            Status = report.Status,
            IsAdmin = isAdmin,
            Username = report.User.Username,
            MunicipalityInfo = municipalityInfo,
            Messages = report.Messages.Select(m => new MessagesModel
            {
                Message = m.Message,
                CreatedAt = m.CreatedAt,
                Username = m.User?.Username ?? "Unknown"
            }).ToList()
            // Include any additional fields as needed
        };

        _logger.LogInformation("Loaded report details successfully for ID: {id}", id);

        // Pass the view model to reported.cshtml
        return View("ReportView", viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> AddMessage(int ReportId, string MessageText)
    {
        // Finn rapporten ved hjelp av ReportId
        var report = await _context.Reports
            .Include(r => r.Messages) // Inkluder eksisterende meldinger
            .FirstOrDefaultAsync(r => r.ReportId == ReportId);

        if (report == null)
        {
            return NotFound();
        }

        // Hent bruker-ID fra pålogget bruker
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null || !int.TryParse(userId, out int parsedUserId))
        {
            return Unauthorized(); // Håndter tilfelle der brukeren ikke er logget inn
        }

        // Opprett en ny melding og legg til i rapporten
        var newMessage = new Messages
        {
            Message = MessageText,
            CreatedAt = DateTime.Now,
            UserId = parsedUserId
        };

        report.Messages.Add(newMessage);

        // Lagre endringer i databasen
        await _context.SaveChangesAsync();

        // Omdiriger til ReportView for å vise oppdatert melding
        return RedirectToAction("ReportView", new { id = ReportId });
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> UpdateStatus(int reportId, Status status)
    {
        // Finn brukeren basert på den autentiserte brukerens ID
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userId, out int parsedUserId))
        {
            return Unauthorized(); // Hvis userId ikke kan parses, returner Unauthorized
        }

        // Hent brukerdata for å sjekke om brukeren er admin
        var user = await _context.Users.FindAsync(parsedUserId);

        if (user == null || !user.IsAdmin) // Sjekk om brukeren eksisterer og er admin
        {
            return Forbid(); // Returner Forbid hvis brukeren ikke er admin
        }

        var report = await _context.Reports.FindAsync(reportId);

        if (report == null)
        {
            return NotFound();
        }

        // Oppdater status
        report.Status = status;

        // Lagre endringene i databasen
        await _context.SaveChangesAsync();

        // Redirect tilbake til ReportView
        return RedirectToAction("ReportView", new { id = reportId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PinReport(int reportId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
        {
            _logger.LogWarning("Unauthorized pin attempt by user.");
            return Json(new { success = false, message = "Unauthorized" });
        }

        _logger.LogInformation("User {UserId} attempting to pin Report {ReportId}", userId, reportId);

        // Check if the report exists
        var report = await _context.Reports.FindAsync(reportId);
        if (report == null)
        {
            _logger.LogWarning("Report {ReportId} not found when User {UserId} attempted to pin.", reportId, userId);
            return Json(new { success = false, message = "Report not found" });
        }

        // Check if already pinned
        var existingPin = await _context.PinnedReports
            .FirstOrDefaultAsync(pr => pr.UserID == userId && pr.ReportID == reportId);

        if (existingPin == null)
        {
            var pin = new PinnedReport { UserID = userId, ReportID = reportId };
            _context.PinnedReports.Add(pin);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User {UserId} successfully pinned Report {ReportId}", userId, reportId);

            return Json(new { success = true, isPinned = true });
        }
        else
        {
            _logger.LogWarning("User {UserId} attempted to pin Report {ReportId}, but it was already pinned.", userId,
                reportId);
            return Json(new { success = false, message = "Report already pinned" });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UnpinReport(int reportId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
        {
            _logger.LogWarning("Unauthorized unpin attempt by user.");
            return Json(new { success = false, message = "Unauthorized" });
        }

        _logger.LogInformation("User {UserId} attempting to unpin Report {ReportId}", userId, reportId);

        var pin = await _context.PinnedReports
            .FirstOrDefaultAsync(pr => pr.UserID == userId && pr.ReportID == reportId);

        if (pin != null)
        {
            _context.PinnedReports.Remove(pin);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User {UserId} successfully unpinned Report {ReportId}", userId, reportId);

            return Json(new { success = true, isPinned = false });
        }

        _logger.LogWarning("User {UserId} attempted to unpin Report {ReportId}, but it was not found.", userId,
            reportId);
        return Json(new { success = false, message = "Pin not found" });
    }

}
