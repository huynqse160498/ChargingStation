using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.DTOs.Chargers
{
    public class ChargerReadDto
    {
        public int ChargerId { get; set; }
        public int StationId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public decimal? PowerKw { get; set; }
        public DateTime? InstalledAt { get; set; }
        public string? ImageUrl { get; set; }

        public string Status { get; set; } = "Online";

        // NEW: mức độ bận, chỉ tính khi đọc (không lưu DB)
        // null khi Status != Online
        public string? Utilization { get; set; } // Idle | Partial | Busy  

        // (tuỳ chọn) số liệu phụ trợ hiển thị
        public int TotalPorts { get; set; }       // NEW
        public int AvailablePorts { get; set; }   // NEW
        public int DisabledPorts { get; set; }    // NEW
    }
}
