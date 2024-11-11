using System.Collections.Generic;

namespace Kartverket.Models
{
    public class ReportOverviewModel
    {
        public List<ReportViewModel> Reports { get; set; } = new List<ReportViewModel>();
        public UserRegistrationModel User { get; set; } // Add this property to hold user information
        
    }   
}