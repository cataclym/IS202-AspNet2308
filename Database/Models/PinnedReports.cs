using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kartverket.Database.Models;

    public class PinnedReport
    {
        [Key, Column(Order = 0)]
        
        public int UserID { get; set; }

        [Key, Column(Order = 1)]
        public int ReportID { get; set; }

        // Navigational properties
        [ForeignKey("UserID")]
        public virtual Users User { get; set; }

        [ForeignKey("ReportID")]
        public virtual Reports Report { get; set; }
    }