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

    [HttpPost]
    public async Task<IActionResult> RegisterMapReport(ReportViewModel model)
    {
        // Logg modellens verdi
        _logger.LogInformation("Model: {Model}", @model);

        // Sjekk om modellen er gyldig
        if (ModelState.IsValid)
        {
            _logger.LogInformation("ModelState er gyldig");
            _logger.LogInformation("GeoJSON-streng: {GeoJson}", model.GeoJsonString);

            // Konverter GeoJSON-strengen til koordinater
            string coordinatesString = ConvertGeoJsonToString(model.GeoJsonString);
            
            ViewBag.Coordinates = coordinatesString;

            // Sjekk om koordinatene er gyldige (hvis det er nødvendig)
            if (string.IsNullOrWhiteSpace(coordinatesString))
            {
                ViewData["ErrorMessage"] = "Ugyldige koordinater fra GeoJSON-strengen.";
                _logger.LogWarning("Konverterte koordinater er ugyldige eller tomme.");
                return View("MapReport", model); // Returner til view hvis koordinatene er ugyldige
            }

            var mapLayers = GetGeoJson(model.GeoJsonString);
            
            // Validering av GeoJSON (valgfritt, avhengig av behov)
            if (mapLayers == null)
            {
                ViewData["ErrorMessage"] = "Du må markere området på kartet.";
                _logger.LogWarning("GeoJSON-data er ugyldig");
                return View("MapReport", model); // Returner til view hvis GeoJSON er ugyldig
            }
            
            MunicipalityCountyNames? municipalityInfo = await _municipalityService.GetMunicipalityFromCoordAsync(mapLayers);
            
            ViewData["MunicipalityInfo"] = municipalityInfo;
            
            _logger.LogInformation("Legger til data i databasen");
            
            
            // Forutsetter at brukeren er autentisert
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            // Konstruerer database-modellen for Reports
            var report = new Reports
            {
                UserId = int.Parse(userId), // Henter brukerens ID fra claims
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
                    UserId = int.Parse(userId)
                });
            }

            // Legg til rapporten i databasen
            _context.Reports.Add(report);

            // Lagre endringer til databasen asynkront
            await _context.SaveChangesAsync();
            _logger.LogInformation("Data har blitt lagret i databasen");

            return View("Reported", model); // Eller til et annet view for å bekrefte lagringen
        }

        _logger.LogWarning("ModelState er ugyldig. Returnerer til view");
        foreach (var state in ModelState)
        {
            foreach (var error in state.Value.Errors)
            {
                _logger.LogWarning($"Valideringsfeil for '{state.Key}': {error.ErrorMessage}");
            }
        }
        
        // Returner samme view med valideringsfeil hvis modellen ikke er gyldig
        return View("MapReport", model);
    }
    
    public async Task<IActionResult> ReportView(int id, ReportViewModel model)
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
        
        // Parse the GeoJsonString into a readable format
        string normalString = ConvertGeoJsonToString(report.GeoJsonString);
        
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

    
    private string ConvertGeoJsonToString(string geoJsonString)
{
    if (string.IsNullOrWhiteSpace(geoJsonString))
    {
        return "No GeoJSON data available";
    }

    try
    {
        var geoJsonObject = JObject.Parse(geoJsonString);
        string type = geoJsonObject["type"]?.ToString();

        switch (type)
        {
            case "FeatureCollection":
                var features = geoJsonObject["features"] as JArray;
                if (features != null)
                {
                    var descriptions = new List<string>();
                    foreach (var feature in features)
                    {
                        var featureDescription = ProcessFeature(feature as JObject);
                        if (!string.IsNullOrEmpty(featureDescription))
                        {
                            descriptions.Add(featureDescription);
                        }
                    }
                    return string.Join("\n", descriptions);
                }
                break;

            case "Feature":
                return ProcessFeature(geoJsonObject);

            // Handle other types if needed

            default:
                return "Unsupported GeoJSON type at root";
        }
    }
    catch (JsonReaderException ex)
    {
        _logger.LogError("Invalid GeoJSON format: {message}", ex.Message);
        return "Invalid GeoJSON data";
    }

    return "Unknown GeoJSON format";
}

private string ProcessFeature(JObject feature)
{
    if (feature == null)
    {
        return null;
    }

    var geometry = feature["geometry"] as JObject;
    if (geometry != null)
    {
        string geomType = geometry["type"]?.ToString();
        var coordinates = geometry["coordinates"] as JArray;

        switch (geomType)
        {
            case "Point":
                if (coordinates != null && coordinates.Count == 2)
                {
                    double longitude = (double)coordinates[0];
                    double latitude = (double)coordinates[1];
                    return $"Point at Latitude: {latitude}, Longitude: {longitude}";
                }
                break;

            case "LineString":
                if (coordinates != null)
                {
                    var points = coordinates.Select(coord => $"({coord[1]}, {coord[0]})");
                    return "Line through points: " + string.Join(" -> ", points);
                }
                break;

            case "Polygon":
                if (coordinates != null)
                {
                    // Polygons can have multiple rings; we'll process the outer ring (first element)
                    var rings = coordinates.First() as JArray;
                    if (rings != null)
                    {
                        var points = rings.Select(coord => $"({coord[1]}, {coord[0]})");
                        return "Polygon with vertices: " + string.Join(", ", points);
                    }
                }
                break;

            // Handle other geometry types as needed

            default:
                return "Unsupported geometry type in feature";
        }
    }

    return "Unknown geometry in feature";
}

    
    

    // Valideringsmetode for GeoJSON (valgfritt)
    private static MapLayersModel? GetGeoJson(string geojson)
    {   
        try
        {
            var obj = JsonSerializer.Deserialize<MapLayersModel>(geojson);

            if (obj?.features.Count > 0) return obj;
        }
        // Hvis Deserialize ikke fullførte, returner null
        catch
        {
            return null;
        }

        return null;
    }
    
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
    public async Task<IActionResult> RegisterUser(UserRegistrationModel userRegistrationModelModel)
    {
        // Sjekk om modellen er gyldig
        if (!ModelState.IsValid) return View(userRegistrationModelModel);
        
        try
        {
            var users = new Users
            {
                Username = userRegistrationModelModel.Username,
                Password = userRegistrationModelModel.Password,
                Email = userRegistrationModelModel.Email,
                Phone = userRegistrationModelModel.Phone,
                IsAdmin = userRegistrationModelModel.IsAdmin,
            };
            // Legger til brukerdata i databasen
            _context.Users.Add(users);

            // Lagre endringer til databasen asynkront
            await _context.SaveChangesAsync();

            // Gå til en suksess- eller bekreftelsesside (eller tilbakemelding på skjema)
            return View("Min Side", userRegistrationModelModel);
        }   
        catch (Exception ex)
        {
            // Logg feil hvis lagringen ikke fungerer
            _logger.LogError(ex, "Feil ved lagring av brukerdata");
            // Returner en feilmelding
            return View("Error");
        }
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
            _logger.LogWarning("User {UserId} attempted to pin Report {ReportId}, but it was already pinned.", userId, reportId);
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

        _logger.LogWarning("User {UserId} attempted to unpin Report {ReportId}, but it was not found.", userId, reportId);
        return Json(new { success = false, message = "Pin not found" });
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
