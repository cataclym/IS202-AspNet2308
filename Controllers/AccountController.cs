using System.Security.Claims;
using Kartverket.Database;
using Kartverket.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kartverket.Controllers;

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
    public IActionResult Login(string returnUrl = null)
    {
        _logger.LogInformation("Checking authentication status");
        
        if (User.Identity is { IsAuthenticated: true })
        {
            _logger.LogInformation("User is authenticated");
            
            _logger.LogInformation("User is authenticated, Name: {UserName}", User.Identity.Name);
            return RedirectToAction("HomePage", "Home");
        }
        else
        {
            _logger.LogInformation("User is NOT authenticated, redirecting to login");
            // Her omdirigeres brukeren til login-siden
            // eller en annen side der autentisering kreves
        }

        // Hvis brukeren ikke er autentisert, vis innloggingssiden
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }
    
    // POST: Behandler innlogging
    [HttpPost]
    public async Task<IActionResult> Login(UserLoginModel userLoginModel, string returnUrl = null)
    {
        if (User.Identity is { IsAuthenticated: true })
        {
            return RedirectToAction("HomePage", "Home");
        }
        Console.WriteLine(ModelState.IsValid);
        if (!ModelState.IsValid) return View("Login", userLoginModel);
        
        // Finn brukeren i databasen basert på brukernavn
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == userLoginModel.Username);


        // Sjekk om brukeren finnes og verifiser passordet
        if (user != null && VerifyPassword(userLoginModel.Password, user.Password))
        {
            // Opprett en liste over påstander (claims) som identifiserer brukeren
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
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
            return RedirectToAction("HomePage", "Home", new { id = user.UserId });
        }

        // Feilhåndtering hvis brukernavn eller passord er feil
        ViewBag.ErrorMessage = "Feil brukernavn eller passord.";
        return View("Login", userLoginModel);
    }
    
    //Funksjon for å logge ut brukeren
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        
        // Logg ut og redirect til login eller startsiden
        return RedirectToAction("Login", "Account");
    }

    
    // Funksjon for å verifisere passord (ved hjelp av hashing)
    private bool VerifyPassword(string inputPassword, string storedPasswordHash)
    {
        // Sammenlign det innsendte passordet med det hashede passordet i databasen
        // Bruk BCrypt for passordhashing
        return BCrypt.Net.BCrypt.Verify(inputPassword, storedPasswordHash);
    }

    
    [HttpPost]
    public async Task<IActionResult> UserRegistration(UserRegistrationModel userRegistrationModelModel)
    {
        // Sjekk om modellen er gyldig (at brukernavn og passord er fylt ut korrekt)
        if (!ModelState.IsValid)
        {
            // Hvis noe er galt med input (f.eks. passord for kort), returner til registreringssiden med feilmeldinger
            return View("UserRegistration", userRegistrationModelModel);
        }

        // Hashe passordet før det lagres i databasen
        userRegistrationModelModel.Password = BCrypt.Net.BCrypt.HashPassword(userRegistrationModelModel.Password);

        // Legg til brukeren i databasen
        _context.Users.Add(userRegistrationModelModel);
        await _context.SaveChangesAsync(); // Lagrer endringene i databasen

        // Returner til en side for å bekrefte at registreringen er vellykket
        return RedirectToAction("HomePage", "Home", new { id = userRegistrationModelModel.UserId }); // Omdirigerer til brukerens profilside, for eksempel
    }

// GET: Viser registreringsskjemaet
    [HttpGet]
    public IActionResult UserRegistration()
    {
        // Brukeren er ikke autentisert
        // Brukeren er autentisert
        Console.WriteLine(User.Identity is { IsAuthenticated: true } ? "User is authenticated" : "User is not authenticated");
        return View();
    }
    
    [Authorize]
    public IActionResult AdminReview()
    {
        return View();
    }

}