using Microsoft.AspNetCore.Mvc;
using Kartverket.Models; // Inkluder riktig namespace for ReportViewModel
using System.Security.Claims; // For å hente userId

public class HomepageViewModel
{
    public List<ReportViewModel> Reports { get; set; }
}

public IActionResult Homepage()
{
    // Hent userId fra brukerens claims
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

    // Hent rapportene for den aktuelle brukeren
    var reports = _context.Reports
        .Where(r => r.UserId == userId)
        .OrderBy(r => r.CreatedAt)
        .Select(r => new ReportViewModel
        {
            ReportId = r.ReportId,
            Message = r.Message,
            Status = r.Status
        })
        .ToList();

    // Lag en instans av HomepageViewModel med rapportene
    var viewModel = new HomepageViewModel
    {
        Reports = reports
    };

    // Returner view med modellen
    return View(viewModel);
}