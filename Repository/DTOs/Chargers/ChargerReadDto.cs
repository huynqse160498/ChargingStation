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
        public string Status { get; set; } = string.Empty;
        public DateTime? InstalledAt { get; set; }
        public string? ImageUrl { get; set; }
    }
}
