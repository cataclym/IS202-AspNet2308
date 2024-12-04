using Kartverket.Database;
using Kartverket.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

namespace Kartverket;

/// <summary>
/// Representerer hovedprogrammet for applikasjonen.
/// Programklassen inneholder hovedmetoden, som er inngangspunktet for applikasjonen.
/// </summary>
public class Program
{
    private static WebApplication? App { get; set; }
    private static WebApplicationBuilder? Builder { get; set; }

    public static void Main(string[] args)
    {
        // Initialiserer builder instansen
        Builder = WebApplication.CreateBuilder(args);

        // Legg til env variabler
        Builder.Configuration.AddEnvironmentVariables();
        
        // Registrerer alle tjenester i builder
        RegisterServices();

        // Lag en instans av webapp fra builderen.
        App = Builder.Build();
        
        // Configure the HTTP request pipeline.
        if (!App.Environment.IsDevelopment())
        {
            App.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            App.UseHsts();
        }

        // Denne legger autentisering i pipelinen
        App.UseAuthentication();
        App.UseHttpsRedirection();
        App.UseStaticFiles();
        App.UseRouting();
        App.UseAntiforgery();
        
        // Autorisasjon må være etter autentisering
        App.UseAuthorization();
        App.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

        RunMigrations();

        App.Run();
    }

    /// <summary>
    /// Statisk metode for å registrere alle tjenestene samlet.
    /// </summary>
    private static void RegisterServices()
    {
        // Konfigurer ApplicationDbContext her
        // Denne henter DefaultConnection fra appsettings.json
        // og er konfigurert for MySQL og MariaDB
        Builder!.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseMySql(Builder.Configuration.GetConnectionString("DefaultConnection"),
                new MariaDbServerVersion(new Version(11, 5, 2))));
        
        // Registrer GeoJsonService som en tjeneste
        Builder.Services.AddScoped<GeoJsonService>();
        
        // Registrer HttpClient for KommuneInfoService og StedsNavnService
        Builder.Services.AddHttpClient<IMunicipalityService, MunicipalityService>(client =>
        {
            client.BaseAddress = new Uri("https://api.kartverket.no/kommuneinfo/v1");
        });
        
        // Registrer UserService
        Builder.Services.AddScoped<IUserService, UserService>();

        // Registrer IHttpContextAccessor
        Builder.Services.AddHttpContextAccessor();
        
        Builder.Services.AddAuthorizationBuilder()
                    .AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));

        // Add services to the container.
        Builder.Services.AddControllersWithViews();
        AddCookies();
    }

    /// <summary>
    /// Legger til og definerer instillinger til cookies
    /// </summary>
    /// <param name="expiration"> En verdi i timer på hvor lenge en cookie skal vare. Standard valg er 2.</param>
    private static void AddCookies(int expiration = 2)
    {
        // Legg til autentiseringstjenester med cookies
        Builder!.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/Account/Login"; // Sti til innloggingssiden
                options.ExpireTimeSpan = TimeSpan.FromHours(expiration); // Hvor lenge cookien varer
                options.SlidingExpiration = true; // Fornyer utløpstiden når brukeren er aktiv
                options.Cookie.HttpOnly = true; // Cookie er kun tilgjengelig for serveren, ikke JavaScript
                options.Cookie.SecurePolicy = CookieSecurePolicy.None; // Bruk HTTPS i produksjon
                options.Cookie.SameSite = SameSiteMode.Lax; // Definerer SameSite-policy for cookien
            });

        Builder.Services.AddAntiforgery(options =>
        {
            options.Cookie.SameSite = SameSiteMode.Lax; // Endre til None eller Lax om nødvendig
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // Endre etter behov
        });
    }

    /// <summary>
    /// Oppdager og kjører migrasjoner som ikke er registert i databasen
    /// Feil oppstår om migrasjoner ikke har blitt fulgt samtidig som databasen er oppdatert 
    /// </summary>
    private static void RunMigrations()
    {
        using var scope = App!.Services.CreateScope();
        var services = scope.ServiceProvider;

        var context = services.GetRequiredService<ApplicationDbContext>();
        if (context.Database.GetPendingMigrations().Any()) context.Database.Migrate();
    }
    
}