using System.ComponentModel.DataAnnotations;

namespace Repositories.DTOs
{
    public class GuestChargingStartDto
    {
        [Required] public string PortCode { get; set; } = default!;
        [Required] public string LicensePlate { get; set; } = default!;
        public string? VehicleType { get; set; } // "Car"/"Motorbike" (staff có thể nhập)
    }

    public class GuestChargingEndDto
    {
        [Required] public int ChargingSessionId { get; set; }
        [Range(0, 100)] public int EndSoc { get; set; }
    }
}
