using Repositories.DTOs;
using Repositories.Interfaces;
using Repositories.Models;
using Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Services.Implementations
{
    public class ChargingSessionService : IChargingSessionService
    {
        private readonly IChargingSessionRepository _sessionRepo;
        private readonly IPricingRuleRepository _pricingRepo;
        private readonly IBookingRepository _bookingRepo;
        private readonly IPortRepository _portRepo;
        private readonly IVehicleRepository _vehicleRepo;
        private readonly IInvoiceRepository _invoiceRepo;
        private readonly Random _rand = new();

        public ChargingSessionService(
            IChargingSessionRepository sessionRepo,
            IPricingRuleRepository pricingRepo,
            IBookingRepository bookingRepo,
            IPortRepository portRepo,
            IVehicleRepository vehicleRepo,
            IInvoiceRepository invoiceRepo)
        {
            _sessionRepo = sessionRepo;
            _pricingRepo = pricingRepo;
            _bookingRepo = bookingRepo;
            _portRepo = portRepo;
            _vehicleRepo = vehicleRepo;
            _invoiceRepo = invoiceRepo;
        }

        // 🕐 Xác định khung giờ hiện tại
        private string GetCurrentTimeRange()
        {
            int hour = DateTime.Now.Hour;
            if (hour >= 22 || hour < 6) return "Low";
            if (hour >= 6 && hour < 17) return "Normal";
            return "Peak";
        }

        // ============================================================
        // 🔹 Bắt đầu phiên sạc
        // ============================================================
        public async Task<ChargingSession> StartSessionAsync(ChargingSessionCreateDto dto)
        {
            int portId;
            int vehicleId;
            int customerId = dto.CustomerId;

            // 1️⃣ Nếu có Booking
            if (dto.BookingId.HasValue)
            {
                var booking = await _bookingRepo.GetByIdAsync(dto.BookingId.Value)
                    ?? throw new Exception("Không tìm thấy Booking.");

                if (booking.CustomerId != dto.CustomerId)
                    throw new Exception("Booking không thuộc khách hàng này.");

                if (booking.Status is not ("Pending" or "Confirmed"))
                    throw new Exception("Booking không hợp lệ để bắt đầu sạc.");

                portId = booking.PortId;
                vehicleId = booking.VehicleId;

                // Đánh dấu booking đang sử dụng
                booking.Status = "InProgress";
                await _bookingRepo.UpdateAsync(booking);
            }
            else
            {
                // 2️⃣ Không có Booking
                if (!dto.PortId.HasValue)
                    throw new Exception("Phải chọn PortId khi không có Booking.");

                var port = await _portRepo.GetByIdAsync(dto.PortId.Value)
                    ?? throw new Exception("Không tìm thấy cổng sạc.");

                if (port.Status != "Available")
                    throw new Exception("Cổng sạc không khả dụng.");

                // Chuyển trạng thái cổng sang đang sử dụng
                port.Status = "InUse";
                await _portRepo.UpdateAsync(port);

                portId = port.PortId;
                vehicleId = dto.VehicleId;
            }

            // 3️⃣ Xác định PricingRule
            var portEntity = await _portRepo.GetByIdAsync(portId)
                ?? throw new Exception("Không tìm thấy port.");
            var charger = portEntity.Charger
                ?? throw new Exception("Không tìm thấy charger.");
            var vehicle = await _vehicleRepo.GetByIdAsync(vehicleId)
                ?? throw new Exception("Không tìm thấy xe.");

            string timeRange = GetCurrentTimeRange();

            // Lấy PricingRule phù hợp theo ChargerType + PowerKw + TimeRange
            var rule = await _pricingRepo.GetAll()
                .Where(x =>
                    x.ChargerType == charger.Type &&
                    x.PowerKw == charger.PowerKw &&
                    x.TimeRange == timeRange &&
                    x.Status == "Active")
                .FirstOrDefaultAsync()
                ?? throw new Exception($"Không tìm thấy PricingRule cho {charger.Type} - {charger.PowerKw}kW ({timeRange}).");

            // 4️⃣ Tạo mới phiên sạc
            var session = new ChargingSession
            {
                BookingId = dto.BookingId, // null nếu không có booking
                CustomerId = customerId,
                VehicleId = vehicleId,
                PortId = portId,
                PricingRuleId = rule.PricingRuleId,
                StartSoc = _rand.Next(20, 80),
                StartedAt = DateTime.Now,
                CreatedAt = DateTime.Now,
                Status = "Charging"
            };

            await _sessionRepo.AddAsync(session);

            // Đảm bảo port trạng thái "InUse"
            var portToUpdate = await _portRepo.GetByIdAsync(portId);
            if (portToUpdate != null)
            {
                portToUpdate.Status = "InUse";
                await _portRepo.UpdateAsync(portToUpdate);
            }

            return session;
        }

        // ============================================================
        // 🔹 Kết thúc phiên sạc
        // ============================================================
        public async Task<ChargingSession> EndSessionAsync(ChargingSessionEndDto dto)
        {
            var session = await _sessionRepo.GetByIdAsync(dto.ChargingSessionId)
                ?? throw new Exception("Không tìm thấy phiên sạc.");

            var rule = await _pricingRepo.GetByIdAsync(session.PricingRuleId)
                ?? throw new Exception("Không tìm thấy PricingRule.");

            int endSoc = dto.EndSoc ?? new Random().Next((session.StartSoc ?? 50) + 10, 101);
            session.EndSoc = endSoc;
            session.EndedAt = DateTime.Now;
            session.DurationMin = (int)(DateTime.Now - session.StartedAt!.Value).TotalMinutes;
            session.IdleMin = _rand.Next(0, 10);
            session.EnergyKwh = Math.Round(((endSoc - (session.StartSoc ?? 50)) * 0.4M), 2);

            // 💰 Tính tiền
            session.Subtotal = session.EnergyKwh * rule.PricePerKwh + session.IdleMin * rule.IdleFeePerMin;
            session.Tax = session.Subtotal * 0.1M;
            session.Total = session.Subtotal + session.Tax;
            session.Status = "Completed";
            session.UpdatedAt = DateTime.Now;

            await _sessionRepo.UpdateAsync(session);

            // 🔓 Giải phóng port
            var port = await _portRepo.GetByIdAsync(session.PortId);
            if (port != null)
            {
                port.Status = "Available";
                await _portRepo.UpdateAsync(port);
            }

            // 🧾 Cập nhật booking (nếu có)
            if (session.BookingId.HasValue)
            {
                var booking = await _bookingRepo.GetByIdAsync(session.BookingId.Value);
                if (booking != null)
                {
                    booking.Status = "Completed";
                    await _bookingRepo.UpdateAsync(booking);
                }
            }

            // 💳 Thêm vào hóa đơn tháng
            var now = DateTime.Now;
            var invoice = await _invoiceRepo.GetOrCreateMonthlyInvoiceAsync(session.CustomerId, now.Month, now.Year);
            invoice.ChargingSessions.Add(session);
            invoice.Total = (invoice.Total ?? 0) + session.Total;
            await _invoiceRepo.SaveAsync();

            return session;
        }

        // ============================================================
        // 🔹 CRUD cơ bản
        // ============================================================
        public async Task<List<ChargingSession>> GetAllAsync() => await _sessionRepo.GetAllAsync();
        public async Task<ChargingSession?> GetByIdAsync(int id) => await _sessionRepo.GetByIdAsync(id);

        public async Task DeleteAsync(int id)
        {
            var session = await _sessionRepo.GetByIdAsync(id)
                ?? throw new Exception("Không tìm thấy phiên sạc.");
            await _sessionRepo.DeleteAsync(session);
        }
    }
}
