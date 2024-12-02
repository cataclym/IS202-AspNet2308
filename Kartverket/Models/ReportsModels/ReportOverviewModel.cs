using System.Collections.Generic;

namespace Kartverket.Models
{
    public class ReportOverviewModel
    {
        public List<ReportViewModel> Reports { get; set; } = new List<ReportViewModel>();
        public required UserRegistrationModel User { get; set; } // Add this property to hold user information
        
    }   
}