using Kartverket.Data;
using Microsoft.EntityFrameworkCore;
using dotenv.net;
using Kartverket.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

DotEnv.Load();
var envVars = DotEnv.Read();

var builder = WebApplication.CreateBuilder(args);

// Legg til env variabler
builder.Configuration.AddEnvironmentVariables();


// Add services to the container.
// Konfigurer ApplicationDbContext her              
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 39)))); // Bytt til vår versjon av MySQL


// Legg til autentiseringstjenester med cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Sti til innloggingssiden
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // Hvor lenge cookien varer
        options.SlidingExpiration = true; // Fornyer utløpstiden når brukeren er aktiv
        options.Cookie.HttpOnly = true; // Cookie er kun tilgjengelig for serveren, ikke JavaScript
        options.Cookie.SecurePolicy = CookieSecurePolicy.None; // Bruk HTTPS i produksjon
        options.Cookie.SameSite = SameSiteMode.Lax; // Definerer SameSite-policy for cookien
    });

// Add services to the container.
builder.Services.AddControllersWithViews();

// Registrer HttpClient for KommuneInfoService og StedsNavnService
builder.Services.AddHttpClient<MunicipalityService>(client =>
{
    client.BaseAddress = new Uri("https://api.kartverket.no/kommuneinfo/v1");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseAuthentication(); // Denne legger autentisering i pipelinen
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();  // Autorisasjon må være etter autentisering
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});

app.MapControllerRoute(
    "default",
    "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<ApplicationDbContext>();
    if (context.Database.GetPendingMigrations().Any())
    {
        context.Database.Migrate();
    }
}

app.Run();