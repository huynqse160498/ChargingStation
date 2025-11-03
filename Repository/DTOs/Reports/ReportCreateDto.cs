using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.DTOs.Reports
{
    public class ReportCreateDto
    {
        [Required] public int StaffId { get; set; }
        [Required] public int StationId { get; set; }
        public int? ChargerId { get; set; }
        public int? PortId { get; set; }

        [Required] public string Title { get; set; } = null!;
        [Required] public string Description { get; set; } = null!;

        // ví dụ dùng: "Low" | "Medium" | "High" | "Critical"
        [Required] public string Severity { get; set; } = "Low";

        // mặc định "Pending" khi tạo
        public string? Status { get; set; } = "Pending";
    }
}
