using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.DTOs.Reports
{
    public class ReportReadDto
    {
        public int ReportId { get; set; }
        public int StaffId { get; set; }
        public int StationId { get; set; }
        public int? ChargerId { get; set; }
        public int? PortId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = "Low";
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }

        // tiện cho UI
        public string? StaffName { get; set; }
        public string? StationName { get; set; }
        public string? ChargerCode { get; set; }
        public string? PortCode { get; set; }
    }
}
