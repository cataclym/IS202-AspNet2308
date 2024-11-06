using System.Security.Claims;
using Kartverket.Database;
using Kartverket.Database.Models;
using Kartverket.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Kartverket.Services;
using Kartverket.Models.AccountModels;


namespace Kartverket.Controllers;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _context; //tilgang til database
    private readonly ILogger<AccountController> _logger;
    private readonly IUserService _userService;
    
    public AccountController(ApplicationDbContext context, ILogger<AccountController> logger, IUserService userService) //constructor
    {
        _context = context;
        _logger = logger;
        _userService = userService;
    }
    
    // GET: Viser innloggingsskjemaet
    [HttpGet]
    public IActionResult Login(string returnUrl = null)
    {
        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            // Hent brukerinformasjon hvis nødvendig
            var isAdmin = User.IsInRole("Admin");
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (isAdmin)
            {
                return RedirectToAction("AdminDashboard", "Home");
            }
            else if (!string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("MyPage", "Home", new { id = userId });
            }
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }
    
    // POST: Behandler innlogging
// POST: Behandler innlogging
[HttpPost]
public async Task<IActionResult> Login(UserLoginModel userLoginModel, string returnUrl = null)
{
    // Sjekk om modellen er gyldig
    if (!ModelState.IsValid)
    {
        return View("Login", userLoginModel);
    }

    // Bruk UserService for konsistens og eventuelle ekstra logikk
    var user = await _userService.GetUserByUsernameAsync(userLoginModel.Username);

    // Sjekk om brukeren finnes og verifiser passordet
    if (user != null && VerifyPassword(userLoginModel.Password, user.Password))
    {
        // Opprett en liste over påstander (claims) som identifiserer brukeren
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString())
        };

        // Legg til rollekrav hvis brukeren er admin
        if (user.IsAdmin)
        {
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));
        }

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        // Opprett autentiseringsbillett (authentication ticket)
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true, // Husk brukeren mellom økter
            ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30) // Sett varigheten for innlogging
        };

        // Logg inn brukeren ved hjelp av cookies
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties
        );

        // Omdiriger basert på rollen eller ReturnUrl
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        if (user.IsAdmin)
        {
            return RedirectToAction("AdminDashboard", "Home");
        }
        else
        {
            return RedirectToAction("MyPage", "Home", new { id = user.UserId });
        }
    }

    // Feilhåndtering hvis brukernavn eller passord er feil
    ModelState.AddModelError(string.Empty, "Ugyldig brukernavn eller passord.");
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

    [HttpGet]
    public IActionResult UserRegistration(string returnUrl = null)
    {
        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            // Hent brukerinformasjon hvis nødvendig
            var isAdmin = User.IsInRole("Admin");
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (isAdmin)
            {
                return RedirectToAction("AdminDashboard", "Home");
            }
            else if (!string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("MyPage", "Home", new { id = userId });
            }
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }
    
    [HttpPost]
    public async Task<IActionResult> UserRegistration(UserRegistrationModel userRegistrationModelModel)
    {
        if (User.Identity is { IsAuthenticated: true })
        {
            return RedirectToAction("MyPage", "Home");
        }
            
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
        return RedirectToAction("MyPage", "Home", new { id = userRegistrationModelModel.UserId }); // Omdirigerer til brukerens profilside, for eksempel
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

    [HttpGet]
    public IActionResult PasswordView()
    {
        return View("ChangePassword");
    }


    [HttpPost]
    public async Task<IActionResult> ChangePassword(ChangePasswordModel model)
    {
        // Validate inputs and ensure newPassword matches confirmPassword
        if (!ModelState.IsValid)
        {
            ViewBag.ErrorMessage = "Passordendringen mislyktes. Vennligst prøv igjen.";
            return View("ChangePassword", model);
        }

        // Get the current logged-in user's ID
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Retrieve the user from the database
        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId.ToString() == userId);


        if (user == null)
        {
            ViewBag.ErrorMessage = "Brukeren ble ikke funnet.";
            return View("ChangePassword", model);
        }

        // Verify if the current password is correct
        if (!VerifyPassword(model.CurrentPassword, user.Password))
        {
            ViewBag.ErrorMessage = "Nåværende passord er feil.";
            return View("ChangePassword", model);
        }

        // Hash the new password
        user.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);

        // Update the password in the database
        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        // Optionally, sign out the user and ask them to log in again, as the cookie still uses the old password
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        // Re-authenticate user
        var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.Username),
        new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString())
    };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,  // Keeps the user logged in
            ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
        };

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

        ViewBag.SuccessMessage = "Passordet ble endret suksessfullt.";

        // Returnerer ChangePassword-visningen med meldingen
        return View("ChangePassword");
    }

public IActionResult AdminReview()
    {
        return View();
    }

}