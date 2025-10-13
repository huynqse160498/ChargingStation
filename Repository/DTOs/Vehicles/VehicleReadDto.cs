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
        public int CustomerId { get; set; }     // NEW
        public int? CompanyId { get; set; }     // NEW
        public int? CurrentSoc { get; set; }    // NEW

        public string CarMaker { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string LicensePlate { get; set; } = string.Empty;
        public int? ManufactureYear { get; set; }
        public decimal? BatteryCapacity { get; set; }
        public string? ConnectorType { get; set; }
        public string? ImageUrl { get; set; }
        public string? Status { get; set; }
        public string? VehicleType { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
