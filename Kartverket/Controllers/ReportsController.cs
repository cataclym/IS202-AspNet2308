using System.Security.Claims;
using Kartverket.Database;
using Kartverket.Database.Models;
using Kartverket.Models;
using Kartverket.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kartverket.Controllers;

[Authorize]
public class ReportsController : BaseController
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ReportsController> _logger;
    private readonly IMunicipalityService _municipalityService;
    private readonly GeoJsonService _geoJsonService;
    private readonly IUserService _userService;
    
    public ReportsController(
        ApplicationDbContext context,
        ILogger<ReportsController> logger,
        IMunicipalityService municipalityService,
        GeoJsonService geoJsonService, IUserService userService)
    {
        _context = context;
        _logger = logger;
        _municipalityService = municipalityService;
        _geoJsonService = geoJsonService;
        _userService = userService;
    }
    
    // GET: Viser registreringsskjemaet
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> ReportOverview(int id = 0)
    {
        id = _userService.GetUserId(id);
        if (id == 0)
        {
            return RedirectToAction("Login", "Account");
        }

        var user = await _userService.GetUserAsync(id);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        // Hent pinnede rapporter for brukeren
        var pinnedReportIds = await GetPinnedReportsAsync(id);

        var reports = await GetReportsAsync(user, pinnedReportIds);

        var viewModel = new ReportOverviewModel
        {
            Reports = reports,
            User = MapUserToViewModel(user)
        };

        return View(viewModel);
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
                                   .FirstOrDefault() ??
                               "No message",
                Status = r.Status,
                CreatedAt = r.CreatedAt,
                Username = r.User.Username,
                IsPinned = pinnedReportIdsSet.Contains(r.ReportId),
                GeoJsonString = r.GeoJsonString,

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
        IsAdmin = user.IsAdmin,
        Password = user.Password
    };
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
        if (municipalityInfo is not null)
        {
            model.MunicipalityInfo = municipalityInfo;
        }

        var userId = GetUserId();
        
        if (userId == null)
        {
            _logger.LogWarning("User ID could not be retrieved.");
            return Unauthorized();
        }
        
        var report = await SaveReportAsync(model, userId.Value);
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

        return View("ReportSuccess", viewModel); // Eller til et annet view for å bekrefte lagringen
    }
    
    private async Task<Reports> SaveReportAsync(ReportViewModel model, int userId)
    {
        var report = new Reports
        {
            UserId = userId,
            GeoJsonString = model.GeoJsonString,
            CreatedAt = DateTime.Now,
            Messages = !string.IsNullOrWhiteSpace(model.FirstMessage)
                ? new List<Messages>
                {
                    new()
                    {
                        Message = model.FirstMessage,
                        CreatedAt = DateTime.Now,
                        UserId = userId
                    }
                }
                : []
        };

        // Legger til kommuneinfo om tilgjengelig
        if (model.MunicipalityInfo != null) await GetAndSaveMunicipality(report, model.MunicipalityInfo);

        _context.Reports.Add(report);
        await _context.SaveChangesAsync();
        return report;
    }

    public async Task<IActionResult> ReportView(int id)
    {
        _logger.LogInformation("Loading report with ID: {id}", id);

        // Fetch the report by ReportId, including any associated tables
        var report = await _context.Reports
            .Include(r => r.Messages)
            .ThenInclude(m => m.User)
            .Include(r => r.User) // Hent brukerdata for selve rapporten
            .Include(r => r.AssignedAdmin) // Include the assigned admin
            .Include(r => r.Municipality)
            .ThenInclude(m => m.County)
            .FirstOrDefaultAsync(r => r.ReportId == id);

        // Handle the case where the report is not found
        if (report == null)
        {
            _logger.LogWarning("Report with ID {id} not found", id);
            return NotFound(); // Alternatively, redirect to a "not found" page or error view
        }
        
        // Hvis kommune mangler på rapport så prøver vi å hente den på nytt og lagre den 
        if (report.MunicipalityId == null)
        {
            var mapLayers = _geoJsonService.GetGeoJson(report.GeoJsonString);
            var municipalityInfo = await _municipalityService.GetMunicipalityFromCoordAsync(mapLayers);
            if (municipalityInfo != null) report.Municipality = await GetAndSaveMunicipality(report, municipalityInfo);
        }
        
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
            MunicipalityInfo = report.Municipality != null ? new ()
            {
                fylkesnavn = report.Municipality.County.Name,
                fylkesnummer = report.Municipality.County.CountyId.ToString(),
                kommunenavn = report.Municipality.Name,
                kommunenummer = report.Municipality.MunicipalityId.ToString(),
            } : null,
            AssignedAdminId = report.AssignedAdminId,
            AssignedAdminUsername = report.AssignedAdmin?.Username,
            Messages = report.Messages.Select(m => new MessagesModel
            {
                Message = m.Message,
                CreatedAt = m.CreatedAt,
                Username = m.User?.Username ?? "Unknown"
            }).ToList()
            // Include any additional fields as needed
        };

        _logger.LogInformation("Loaded report details successfully for ID: {id}", id);

        // If the user is a normal user, show the regular report view
        return View(User.IsInRole("Admin") ?
            // If the user is an admin, show the admin-specific view
            "AdminReportView" : "ReportView", viewModel);
    }
    
    
    [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> EditMapReport(ReportViewModel model)
{
    if (model.ReportId == 0)
    {
        _logger.LogWarning("Invalid ReportId {ReportId}.", model.ReportId);
        return BadRequest("Invalid Report ID.");
    }

    // Retrieve the existing report from the database, including related Messages
    var report = await _context.Reports
        .Include(r => r.Messages)
        .FirstOrDefaultAsync(r => r.ReportId == model.ReportId);

    if (report == null)
    {
        _logger.LogWarning("Report with ID {ReportId} not found.", model.ReportId);
        return NotFound("Report not found.");
    }

    // Ensure the current user is authorized to edit the report
    var userId = GetUserId();
    if (userId == null || report.UserId != userId.Value)
    {
        _logger.LogWarning("User {UserId} is not authorized to edit report {ReportId}.", userId, model.ReportId);
        return Unauthorized("You are not authorized to edit this report.");
    }

    // Validate the model
    if (!ModelState.IsValid)
    {
        _logger.LogInformation("ModelState is invalid for ReportId {ReportId}.", model.ReportId);
        foreach (var key in ModelState.Keys)
        {
            var state = ModelState[key];
            if (state == null) continue;
            foreach (var error in state.Errors)
            {
                _logger.LogError("Field {Field} Error: {ErrorMessage}", key, error.ErrorMessage);
            }
        }
        return View("ReportView", model);
    }

    _logger.LogInformation("Processing edit for ReportId {ReportId}.", model.ReportId);

    // **Replace the existing GeoJsonString with the new one**
    report.GeoJsonString = model.GeoJsonString;
    
    // **Edit the content of the first message, if it exists**
    var firstMessage = report.Messages.OrderBy(m => m.CreatedAt).FirstOrDefault();
    if (firstMessage != null && !string.IsNullOrWhiteSpace(model.FirstMessage))
    {
        firstMessage.Message = model.FirstMessage;
        _logger.LogInformation("Updated the first message for ReportId {ReportId}.", model.ReportId);
    }
    
    
    // **Save changes to the database**
    try
    {
        await _context.SaveChangesAsync();
        _logger.LogInformation("Successfully updated report with ID {ReportId}.", report.ReportId);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error updating report with ID {ReportId}.", report.ReportId);
        ModelState.AddModelError(string.Empty, "An error occurred while updating the report.");
        return View("ReportView", model);
    }

    // **Redirect to the updated report details page**
    return RedirectToAction("ReportView", new { id = report.ReportId });
}


    [HttpGet]
    public async Task<IActionResult> EditMapReport(int id)
    {
        // Retrieve the report and prepare the model
        var report = await _context.Reports
            .Include(r => r.Messages)
            .FirstOrDefaultAsync(r => r.ReportId == id);

        if (report == null)
        {
            return NotFound();
        }

        // Authorization check (optional)

        var model = new ReportViewModel
        {
            ReportId = report.ReportId,
            GeoJsonString = report.GeoJsonString,
            FirstMessage = report.Messages.FirstOrDefault()?.Message,
            // Other properties...
        };

        ViewBag.IsEdit = true;
        return View("MapReport", model);
    }


    [HttpPost]
    public async Task<IActionResult> AddMessage(int reportId, string messageText)
    {
        // Finn rapporten ved hjelp av ReportId
        var report = await _context.Reports
            .Include(r => r.Messages) // Inkluder eksisterende meldinger
            .FirstOrDefaultAsync(r => r.ReportId == reportId);

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
            Message = messageText,
            CreatedAt = DateTime.Now,
            UserId = parsedUserId
        };

        report.Messages.Add(newMessage);

        // Lagre endringer i databasen
        await _context.SaveChangesAsync();

        // Omdiriger til ReportView for å vise oppdatert melding
        return RedirectToAction("ReportView", new { id = reportId });
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
        _logger.LogInformation("PinReport action invoked with ReportId: {ReportId}", reportId);

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
    
   
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Claim(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var report = await _context.Reports
            .Include(r => r.AssignedAdmin)
            .Include(r => r.Messages)
            .FirstOrDefaultAsync(r => r.ReportId == id);

        if (report == null)
        {
            return NotFound();
        }

        if (report.AssignedAdminId != null)
        {
            return BadRequest("Report has already been claimed.");
        }

        // Map Report to ReportViewModel
        var viewModel = new ReportViewModel
        {
            ReportId = report.ReportId,
            FirstMessage = report.Messages
                               .OrderBy(m => m.CreatedAt)
                               .Select(m => m.Message)
                               .FirstOrDefault() ??
                           "No message",
            GeoJsonString = report.GeoJsonString,
        };
        
        return View(viewModel);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ClaimConfirmed(int id)
    {
        var report = await _context.Reports.FindAsync(id);
        if (report == null)
        {
            return NotFound();
        }

        if (report.AssignedAdminId != null)
        {
            return BadRequest("Report has already been claimed.");
        }

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdClaim, out int currentUserId))
        {
            return Unauthorized();
        }
        
        var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.UserId == currentUserId);

        if (adminUser == null || !adminUser.IsAdmin)
        {
            return Unauthorized();
        }

        // Assign the report to the current admin
        report.AssignedAdminId = adminUser.UserId;

        try
        {
            _context.Update(report);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Report successfully claimed.";
        }
        catch (Exception)
        {
            // Log the error (not shown)
            TempData["ErrorMessage"] = "An error occurred while claiming the report.";
        }

        return RedirectToAction("ReportView", new { id = report.ReportId });
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var report = await _context.Reports
            .Include(r => r.Messages)
            .FirstOrDefaultAsync(r => r.ReportId == id);

        if (report == null)
        {
            return NotFound();
        }

        _context.Reports.Remove(report);
        await _context.SaveChangesAsync();

        return RedirectToAction("ReportOverview");
    }

    private async Task<Municipality> GetAndSaveMunicipality(Reports report, MunicipalityCountyNames municipalityInfo)
    {
        // Henter int fra string
        int municipalityId = int.Parse(municipalityInfo.kommunenummer);
        int countyId = int.Parse(municipalityInfo.fylkesnummer);

        // Prøver å hente relevant fylke og kommune
        var municipality = await _context.Municipality
            .FirstOrDefaultAsync(m => m.MunicipalityId == municipalityId);
        var county = await _context.County
            .FirstOrDefaultAsync(m => m.CountyId == countyId);

        // Hvis kommune eksisterer så kobler vi den til rapporten
        if (municipality is not null)
        {
            report.MunicipalityId = municipality.MunicipalityId;
            report.Municipality = municipality;

            return municipality;
        }

        // Opprett rad i kommune tabellen og fylke tabellen hvis de ikke eksisterer
        Municipality newMunicipality = new()
        {
            MunicipalityId = municipalityId,
            Name = municipalityInfo.kommunenavn,
            CountyId = countyId,
            County = county ?? new()
            {
                CountyId = countyId,
                Name = municipalityInfo.fylkesnavn
            },
        };
        
        // Lagrer den nye relasjonen med Kommune og Fylke tabeller
        _context.Municipality.Add(newMunicipality);
        report.MunicipalityId = newMunicipality.MunicipalityId;
        report.Municipality = newMunicipality;
            
        return report.Municipality;
    }
}


