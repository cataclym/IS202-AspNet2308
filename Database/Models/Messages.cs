using System.ComponentModel.DataAnnotations;

namespace Kartverket.Database.Models;

public class Messages
{
    [Key] public int MessageId { get; set; }
    public string Message { get; set; }
    public int ReportId { get; set; }
}