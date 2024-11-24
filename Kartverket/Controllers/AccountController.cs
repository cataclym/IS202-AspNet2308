using System.Security.Claims;
using Kartverket.Database;
using Kartverket.Database.Models;
using Kartverket.Models;
using Kartverket.Services;
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
    private readonly IUserService _userService;

    public AccountController(ApplicationDbContext context, ILogger<AccountController> logger,
        IUserService userService) //constructor
    {
        _context = context;
        _logger = logger;
        _userService = userService;
    }

    // GET: Viser innloggingsskjemaet
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity is { IsAuthenticated: true })
        {
            // Hent brukerinformasjon hvis nødvendig
            var isAdmin = User.IsInRole("Admin");
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (isAdmin)
                return RedirectToAction("AdminDashboard", "Home");
            if (!string.IsNullOrEmpty(userId)) return RedirectToAction("MyPage", "Home", new { id = userId });
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    // POST: Behandler innlogging
    [HttpPost]
    public async Task<IActionResult> Login(UserLoginModel userLoginModel, string? returnUrl = null)
    {
        // Sjekk om modellen er gyldig
        if (!ModelState.IsValid) return View("Login", userLoginModel);

        // Bruk UserService for konsistens og eventuelle ekstra logikk
        var user = await _userService.GetUserByUsernameAsync(userLoginModel.Username);

        // Sjekk om brukeren finnes og verifiser passordet
        if (user != null && VerifyPassword(userLoginModel.Password, user.Password))
        {
            // Opprett en liste over påstander (claims) som identifiserer brukeren
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.Username),
                new(ClaimTypes.NameIdentifier, user.UserId.ToString())
            };

            // Legg til rollekrav hvis brukeren er admin
            if (user.IsAdmin) claims.Add(new Claim(ClaimTypes.Role, "Admin"));

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
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);

            // Returnerer view basert på om brukeren er Admin (Saksbehandler)
            return user.IsAdmin
                ? RedirectToAction("AdminDashboard", "Home")
                : RedirectToAction("MyPage", "Home", new { id = user.UserId });
        }

        // Feilhåndtering hvis brukernavn eller passord er feil
        ModelState.AddModelError(string.Empty, "Ugyldig brukernavn eller passord.");
        return View("Login", userLoginModel);
    }


    // Funksjon for å logge ut brukeren
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
    
    // TODO beskrivelse
    [HttpGet]
    public IActionResult UserRegistration(string? returnUrl = null)
    {
        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            // Hent brukerinformasjon hvis nødvendig
            var isAdmin = User.IsInRole("Admin");
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (isAdmin) return RedirectToAction("AdminDashboard", "Home");
            if (!string.IsNullOrEmpty(userId)) return RedirectToAction("MyPage", "Home", new { id = userId });
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    // TODO beskrivelse
    [HttpPost]
    public async Task<IActionResult> UserRegistration(UserRegistrationModel userRegistrationModelModel)
    {
        if (User.Identity is { IsAuthenticated: true }) return RedirectToAction("MyPage", "Home");

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
        return RedirectToAction("MyPage", "Home",
            new { id = userRegistrationModelModel.UserId }); // Omdirigerer til brukerens profilside, for eksempel
    }

    // POST: Behandle bruker registrering
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
                IsAdmin = userRegistrationModelModel.IsAdmin
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

    public async Task<IActionResult> ChangePassword(ChangePasswordModel model)
    {
        // Validate model state, which includes ConfirmPassword matching and other annotations.
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Password change failed: Invalid model state.");
            return View("ChangePassword", model);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            _logger.LogWarning("Password change failed: User not authenticated.");
            return View("ChangePassword", model);
        }

        // Retrieve the user from the database
        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId.ToString() == userId);
        if (user == null)
        {
            _logger.LogWarning("Password change failed: User not found in the database.");
            return View("ChangePassword", model);
        }

        // Verify if the current password is correct
        if (!VerifyPassword(model.CurrentPassword, user.Password))
        {
            _logger.LogWarning("Password change failed: Current password is incorrect.");
            ModelState.AddModelError(string.Empty, "Nåværende passord er feil."); // Add error message for the user
            return View("ChangePassword", model);
        }

        // Hash the new password and mark the entity as modified
        user.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
        _context.Entry(user).Property(u => u.Password).IsModified = true;

        try
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("Password updated successfully for user with ID {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating the password for user with ID {UserId}", userId);
            return View("ChangePassword", model);
        }

        // Sign out and re-authenticate the user with the new password
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.NameIdentifier, user.UserId.ToString())
        };
        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2)
        };

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity), authProperties);

        _logger.LogInformation("User with ID {UserId} re-authenticated successfully after password change.", userId);

        return View("ChangePassword");
    }


    [HttpGet]
    public IActionResult ChangePassword()
    {
        return View();
    }

    public IActionResult AdminReview()
    {
        return View();
    }

// Sletting av brukerkonto
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> DeleteUser()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                ModelState.AddModelError("", "Brukeren ble ikke funnet.");
                return View("Error");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId.ToString() == userId);

            if (user == null)
            {
                ModelState.AddModelError("", "Brukeren ble ikke funnet.");
                return View("Error");
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Sett TempData før omdirigering
            TempData["DeletionMessage"] = "Brukerkontoen din er nå slettet.";

            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Feil ved sletting av brukerdata");
            return View("Error");
        }
    }
}