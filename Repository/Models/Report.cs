using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Repositories.Models
{
    public class Report
    {
        [Key]
        public int ReportId { get; set; }

        
        public int StaffId { get; set; }

        public int StationId { get; set; }

        public int? ChargerId { get; set; }
        public int? PortId { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string Severity { get; set; }

        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? ResolvedAt { get; set; }

        public virtual Account Staff { get; set; }

        public virtual Station Station { get; set; }

        public virtual Charger Charger { get; set; }

        public virtual Port Port { get; set; }
    }
}
