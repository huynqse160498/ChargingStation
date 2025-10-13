using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.DTOs.Vehicles
{
    public class VehicleCreateDto
    {
        [Required, MaxLength(100)]
        public string CarMaker { get; set; } = string.Empty;

        [Required, MaxLength(120)]
        public string Model { get; set; } = string.Empty;

        
        [Required, MaxLength(50)]
        public string LicensePlate { get; set; } = string.Empty;

        public int? ManufactureYear { get; set; }

       
        public decimal? BatteryCapacity { get; set; }

        public string? ConnectorType { get; set; }
        public string? ImageUrl { get; set; }

        public string? VehicleType { get; set; }


    }
}
