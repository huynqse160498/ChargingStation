namespace Repositories.DTOs
{
    // ✅ Dùng khi bắt đầu phiên sạc
    public class ChargingSessionCreateDto
    {
        public int CustomerId { get; set; }
        public int VehicleId { get; set; }
        public int? BookingId { get; set; } // Có thể null
        public int? PortId { get; set; }    // Bắt buộc nếu không có Booking
    }

    // ✅ Dùng khi kết thúc phiên sạc
    public class ChargingSessionEndDto
    {
        public int ChargingSessionId { get; set; }
        public int? EndSoc { get; set; }
    }

    // ✅ DTO trả về thông tin chi tiết
    public class ChargingSessionDto
    {
        public int ChargingSessionId { get; set; }
        public int CustomerId { get; set; }
        public int VehicleId { get; set; }
        public int PortId { get; set; }
        public int? BookingId { get; set; }
        public string VehicleType { get; set; }
        public string TimeRange { get; set; }
        public decimal EnergyKwh { get; set; }
        public decimal Total { get; set; }
        public int DurationMin { get; set; }
        public int IdleMin { get; set; }
        public int StartSoc { get; set; }
        public int? EndSoc { get; set; }
        public string Status { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
    }
}
