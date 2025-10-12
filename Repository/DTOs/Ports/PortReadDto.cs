using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.DTOs.Ports
{
    public class PortReadDto
    {
        public int PortId { get; set; }
        public int ChargerId { get; set; }
        public string ConnectorType { get; set; } = string.Empty;
        public decimal? MaxPowerKw { get; set; }
        public string Status { get; set; } = "Available";
        public string? ImageUrl { get; set; }
    }
}
