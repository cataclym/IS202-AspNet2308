using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Kartverket.Data;
using Kartverket.Models;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;



public class AccountController : Controller
{
    private readonly ApplicationDbContext _context; //tilgang til database
    private readonly ILogger<AccountController> _logger;
    
    public AccountController(ApplicationDbContext context, ILogger<AccountController> logger) //constructor
    {
        _context = context;
        _logger = logger;
    }
    
    // GET: Viser innloggingsskjemaet
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }
    
    // POST: Behandler innlogging
    [HttpPost]
    public async Task<IActionResult> Login(Users usersModel)
    {
        if (!ModelState.IsValid) return View("Login", usersModel);

        // Finn brukeren i databasen basert på brukernavn
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.UserName == usersModel.UserName);

        // Sjekk om brukeren finnes og verifiser passordet
        if (user != null && VerifyPassword(usersModel.Password, user.Password))
        {
            // Opprett en liste over påstander (claims) som identifiserer brukeren
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            // Opprett en autentiseringsbillett (authentication ticket)
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true, // Husk brukeren mellom økter
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30) // Sett varigheten for innlogging
            };

            // Logg inn brukeren ved hjelp av cookies
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
            // Logg inn brukeren og omdiriger til ønsket side (for eksempel forsiden)
            return RedirectToAction("Homepage", "Home", new { id = user.UserId });
        }

        // Feilhåndtering hvis brukernavn eller passord er feil
        ViewBag.ErrorMessage = "Feil brukernavn eller passord.";
        return View("Login", usersModel);
    }
    
    // Funksjon for å verifisere passord (ved hjelp av hashing)
    private bool VerifyPassword(string inputPassword, string storedPasswordHash)
    {
        // Sammenlign det innsendte passordet med det hashede passordet i databasen
        // Bruk BCrypt for passordhashing
        return BCrypt.Net.BCrypt.Verify(inputPassword, storedPasswordHash);
    }

    
    [HttpPost]
    public async Task<IActionResult> UserRegistration(Users usersModel)
    {
        // Sjekk om modellen er gyldig (at brukernavn og passord er fylt ut korrekt)
        if (!ModelState.IsValid)
        {
            // Hvis noe er galt med input (f.eks. passord for kort), returner til registreringssiden med feilmeldinger
            return View("UserRegistration", usersModel);
        }

        // Hashe passordet før det lagres i databasen
        usersModel.Password = BCrypt.Net.BCrypt.HashPassword(usersModel.Password);

        // Legg til brukeren i databasen
        _context.Users.Add(usersModel);
        await _context.SaveChangesAsync(); // Lagrer endringene i databasen

        // Returner til en side for å bekrefte at registreringen er vellykket
        return RedirectToAction("HomePage", "Home", new { id = usersModel.UserId }); // Omdirigerer til brukerens profilside, for eksempel
    }

// GET: Viser registreringsskjemaet
    [HttpGet]
    public IActionResult UserRegistration()
    {
        return View();
    }
    
}