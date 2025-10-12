using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.DTOs.Vehicles
{
    public class VehicleReadDto
    {
        public int VehicleId { get; set; }
        public string CarMaker { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string LicensePlate { get; set; } = string.Empty;
        public int? ManufactureYear { get; set; }
        public decimal? BatteryCapacity { get; set; }    
        public string? ConnectorType { get; set; }
        public string? ImageUrl { get; set; }
        public string? Status { get; set; }
    }
}
