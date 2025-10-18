using System.ComponentModel.DataAnnotations;

namespace Repositories.DTOs.Vehicles
{
    public class VehicleCreateDto
    {
        [Required]
        public int CustomerId { get; set; }

        public int? CompanyId { get; set; }

        [Required, MaxLength(100)]
        public string CarMaker { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Model { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string LicensePlate { get; set; } = string.Empty;

        public decimal? BatteryCapacity { get; set; }

        [Range(0, 100)]
        public int? CurrentSoc { get; set; }

        [MaxLength(50)]
        public string ConnectorType { get; set; } = string.Empty;

        [Range(1900, 9999)]
        public int? ManufactureYear { get; set; }

        [MaxLength(255)]
        public string? ImageUrl { get; set; }

        public string VehicleType { get; set; } = string.Empty;

        // NEW: chỉ 4 trạng thái hợp lệ
        [MaxLength(20)]
        public string Status { get; set; } = "Active"; // Active | Inactive | Blacklisted | Retired
    }
}
