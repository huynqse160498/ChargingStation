using Microsoft.EntityFrameworkCore;
using Repositories.DTOs;
using Repositories.Interfaces;
using Repositories.Models;
using Services.Interfaces;

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
        private readonly ISubscriptionRepository _subscriptionRepo;

        public ChargingSessionService(
            IChargingSessionRepository sessionRepo,
            IPricingRuleRepository pricingRepo,
            IBookingRepository bookingRepo,
            IPortRepository portRepo,
            IVehicleRepository vehicleRepo,
            IInvoiceRepository invoiceRepo,
            ISubscriptionRepository subscriptionRepo)
        {
            _sessionRepo = sessionRepo;
            _pricingRepo = pricingRepo;
            _bookingRepo = bookingRepo;
            _portRepo = portRepo;
            _vehicleRepo = vehicleRepo;
            _invoiceRepo = invoiceRepo;
            _subscriptionRepo = subscriptionRepo;
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
                    throw new Exception("Cổng sạc đang được sử dụng hoặc bị khóa.");

                port.Status = "InUse";
                await _portRepo.UpdateAsync(port);

                portId = port.PortId;
                vehicleId = dto.VehicleId;
            }

            // 3️⃣ Xác định PricingRule theo Charger
            var portEntity = await _portRepo.GetByIdAsync(portId)
                ?? throw new Exception("Không tìm thấy Port.");
            var charger = portEntity.Charger
                ?? throw new Exception("Không tìm thấy Charger của port.");
            var vehicle = await _vehicleRepo.GetByIdAsync(vehicleId)
                ?? throw new Exception("Không tìm thấy xe.");

            string timeRange = GetCurrentTimeRange();

            var rule = await _pricingRepo.GetAll()
                .Where(x =>
                    x.ChargerType == charger.Type &&
                    x.PowerKw == charger.PowerKw &&
                    x.TimeRange == timeRange &&
                    x.Status == "Active")
                .FirstOrDefaultAsync()
                ?? throw new Exception($"Không có PricingRule cho {charger.Type} - {charger.PowerKw}kW ({timeRange}).");

            // 4️⃣ Tạo mới phiên sạc
            var session = new ChargingSession
            {
                BookingId = dto.BookingId, // có thể null
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
            return session;
        }

        // ============================================================
        // 🔹 Kết thúc phiên sạc
        // ============================================================
        public async Task<ChargingSession> EndSessionAsync(ChargingSessionEndDto dto)
        {
            // ============================
            // 🔹 1. Lấy dữ liệu cơ bản
            // ============================
            var session = await _sessionRepo.GetByIdAsync(dto.ChargingSessionId)
                ?? throw new Exception("Không tìm thấy phiên sạc.");

            var rule = await _pricingRepo.GetByIdAsync(session.PricingRuleId)
                ?? throw new Exception("Không tìm thấy PricingRule.");

            var vehicle = await _vehicleRepo.GetByIdAsync(session.VehicleId)
                ?? throw new Exception("Không tìm thấy xe cho phiên sạc này.");

            if (vehicle.BatteryCapacity == null || vehicle.BatteryCapacity <= 0)
                throw new Exception("Dung lượng pin (BatteryCapacity) của xe không hợp lệ.");

            int startSoc = session.StartSoc ?? 50;
            int endSoc = dto.EndSoc ?? new Random().Next(startSoc + 10, 101);
            if (endSoc <= startSoc)
                throw new Exception("SOC kết thúc phải lớn hơn SOC bắt đầu.");

            // ============================
            // 🔹 2. Tính năng lượng sạc (kWh)
            // ============================
            session.EnergyKwh = Math.Round(
                (vehicle.BatteryCapacity.Value * (endSoc - startSoc) / 100M),
                2
            );

            session.EndSoc = endSoc;
            session.EndedAt = DateTime.Now;
            session.DurationMin = (int)(DateTime.Now - session.StartedAt!.Value).TotalMinutes;
            session.IdleMin = _rand.Next(0, 10);

            // ============================
            // 🔹 3. Áp dụng ưu đãi Subscription (nếu có)
            // ============================
            var activeSub = await _subscriptionRepo.GetActiveByCustomerOrCompanyAsync(session.CustomerId, vehicle.CompanyId);

            decimal pricePerKwh = rule.PricePerKwh;
            decimal idleFeePerMin = rule.IdleFeePerMin;
            int freeIdle = 0;
            decimal discountPercent = 0M;

            if (activeSub != null && activeSub.SubscriptionPlan != null)
            {
                freeIdle = activeSub.SubscriptionPlan.FreeIdleMinutes ?? 0;
                discountPercent = activeSub.SubscriptionPlan.DiscountPercent ?? 0;
            }

            int actualIdle = session.IdleMin ?? 0;
            int chargeableIdle = Math.Max(actualIdle - freeIdle, 0);

            decimal rawSubtotal = (session.EnergyKwh ?? 0M) * pricePerKwh
                                + chargeableIdle * idleFeePerMin;

            // 🔸 Áp dụng giảm giá phần trăm nếu có
            if (discountPercent > 0)
                rawSubtotal -= rawSubtotal * (discountPercent / 100M);

            session.Subtotal = Math.Round(rawSubtotal, 2);

            // ============================
            // 🔹 4. Tính thuế và tổng tiền
            // ============================
            decimal subtotalValue = session.Subtotal ?? 0M;
            session.Tax = Math.Round(subtotalValue * 0.1M, 2); // VAT 10%
            session.Total = subtotalValue + session.Tax;

            session.Status = "Completed";
            session.UpdatedAt = DateTime.Now;
            await _sessionRepo.UpdateAsync(session);

            // ============================
            // 🔹 5. Giải phóng cổng sạc
            // ============================
            var port = await _portRepo.GetByIdAsync(session.PortId);
            if (port != null)
            {
                port.Status = "Available";
                await _portRepo.UpdateAsync(port);
            }

            // ============================
            // 🔹 6. Cập nhật Booking (nếu có)
            // ============================
            if (session.BookingId.HasValue)
            {
                var booking = await _bookingRepo.GetByIdAsync(session.BookingId.Value);
                if (booking != null)
                {
                    booking.Status = "Completed";
                    await _bookingRepo.UpdateAsync(booking);
                }
            }

            // ============================
            // 🔹 7. Gắn session vào hóa đơn tháng
            // ============================
            var now = DateTime.Now;
            var invoice = await _invoiceRepo.GetOrCreateMonthlyInvoiceAsync(session.CustomerId, now.Month, now.Year);

            // ⚠️ Nếu invoice đã thanh toán, tạo hóa đơn mới
            if (invoice.Status == "Paid")
            {
                invoice = new Invoice
                {
                    CustomerId = session.CustomerId,
                    CompanyId = vehicle.CompanyId,
                    BillingMonth = now.Month,
                    BillingYear = now.Year,
                    CreatedAt = DateTime.Now,
                    Status = "Unpaid",
                    IsMonthlyInvoice = true
                };
                await _invoiceRepo.AddAsync(invoice);
            }

            // 🔗 Gắn Subscription (nếu có)
            if (activeSub != null)
                invoice.SubscriptionId = activeSub.SubscriptionId;

            // 🔗 Gắn session vào invoice
            invoice.ChargingSessions.Add(session);
            invoice.Total = (invoice.Total ?? 0M) + session.Total;
            invoice.UpdatedAt = DateTime.Now;

            await _invoiceRepo.SaveAsync();

            // ============================
            // ✅ 8. Hoàn tất
            // ============================
            return session;
        }


        // ============================================================
        // 🔹 CRUD
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
