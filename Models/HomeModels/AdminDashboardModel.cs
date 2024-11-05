namespace Kartverket.Models;

public class AdminDashboardModel
{
    public string? Username { get; set; }
        
    public string? Email { get; set; }
    
    public string? Phone { get; set; }
    public int UnprocessedReportsCount { get; set; }
    public int ReportsTodayCount { get; set; }
    public int ProcessedReportsCount { get; set; }
    public int ReportsUnderTreatmentCount { get; set; }
}