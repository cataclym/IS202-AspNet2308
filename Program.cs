using Kartverket.Database;
using Kartverket.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

namespace Kartverket;

public class Program
{
    private static WebApplication App { get; set; }
    private static WebApplicationBuilder Builder { get; set; }

    public static void Main(string[] args)
    {
        Builder = WebApplication.CreateBuilder(args);
        
        // Registrer GeoJsonService som en tjeneste
        Builder.Services.AddScoped<GeoJsonService>();

        // Legg til env variabler
        Builder.Configuration.AddEnvironmentVariables();

        // Add services to the container.
        // Konfigurer ApplicationDbContext her              
        Builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseMySql(Builder.Configuration.GetConnectionString("DefaultConnection"),
                new MariaDbServerVersion(new Version(11, 5, 2)))); // Bytt til vår versjon av MySQL
        
        // Registrer IUserService og UserService
        Builder.Services.AddScoped<IUserService, UserService>();

        // Registrer IHttpContextAccessor
        Builder.Services.AddHttpContextAccessor();
        
        Builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
        });

        // Add services to the container.
        Builder.Services.AddControllersWithViews();
        
        // Add services to the container.
        Builder.Services.AddControllersWithViews();
        AddCookies();
        AddRoutes();

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
        // Autorisasjon må være etter autentisering
        App.UseAuthorization();
        App.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

        RunMigrations();

        App.Run();
    }

    private static void AddRoutes()
    {
        // Registrer HttpClient for KommuneInfoService og StedsNavnService
        Builder.Services.AddHttpClient<MunicipalityService>(client =>
        {
            client.BaseAddress = new Uri("https://api.kartverket.no/kommuneinfo/v1");
        });
    }

    private static void AddCookies()
    {
        // Legg til autentiseringstjenester med cookies
        Builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/Account/Login"; // Sti til innloggingssiden
                options.ExpireTimeSpan = TimeSpan.FromHours(2); // Hvor lenge cookien varer
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

    private static void RunMigrations()
    {
        using var scope = App.Services.CreateScope();
        var services = scope.ServiceProvider;

        var context = services.GetRequiredService<ApplicationDbContext>();
        if (context.Database.GetPendingMigrations().Any()) context.Database.Migrate();
    }
}