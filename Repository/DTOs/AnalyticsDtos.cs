namespace Repositories.DTOs
{
    // ── Common aggregates
    public class AnalyticsSummaryDto
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public int SessionCount { get; set; }
        public decimal EnergyKwh { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }
        public int DurationMin { get; set; }
        public int IdleMin { get; set; }
        public decimal AvgPricePerKwh { get; set; } // Total/SubtotalKwh-aware (safe guarded)
    }

    // ── Revenue source split
    public class RevenueSourceDto
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal CustomerTotal { get; set; }   // sessions có CustomerId != null
        public decimal CompanyTotal { get; set; }    // sessions có CompanyId != null
        public decimal GuestTotal { get; set; }      // sessions CustomerId == null && CompanyId == null
    }

    // ── Generic breakdown line
    public class BreakdownItemDto
    {
        public string Key { get; set; } = default!;  // CompanyName / StationCode / ChargerCode / PortCode / ConnectorType / VehicleType / TimeRange...
        public string? Key2 { get; set; }            // optional (ví dụ StationCode + ChargerCode)
        public int SessionCount { get; set; }
        public decimal EnergyKwh { get; set; }
        public decimal Total { get; set; }
        public int DurationMin { get; set; }
        public int IdleMin { get; set; }
    }

    // ── Vehicle breakdown (thêm VehicleId / Plate)
    public class VehicleBreakdownItemDto : BreakdownItemDto
    {
        public int VehicleId { get; set; }
        public string? LicensePlate { get; set; }
        public string? VehicleType { get; set; }
    }

    // ── Utilization per hardware
    public class HardwareUtilDto
    {
        public string Scope { get; set; } = "Port"; // "Port" | "Charger" | "Station"
        public string Code { get; set; } = default!; // PortCode / ChargerCode / StationCode
        public string? ParentCode { get; set; }      // Parent: (for Port -> Charger, for Charger -> Station)
        public int SessionCount { get; set; }
        public int ChargingMinutes { get; set; }     // sum(DurationMin)
        public decimal EnergyKwh { get; set; }
        public decimal Utilization { get; set; }     // ChargingMinutes / totalMinutesOfMonth
    }

    // ── Top lists
    public class TopListDto
    {
        public List<BreakdownItemDto> TopActive { get; set; } = new();
        public List<BreakdownItemDto> UnderUtilized { get; set; } = new();
        public List<BreakdownItemDto> ZeroActivity { get; set; } = new();
    }
}
