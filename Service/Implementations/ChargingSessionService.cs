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
        private readonly ISubscriptionRepository _subscriptionRepo;
        private readonly Random _rand = new();

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

        // ============================================================
        // 🔹 Xác định khung giờ hiện tại
        // ============================================================
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

            if (dto.BookingId.HasValue)
            {
                var booking = await _bookingRepo.GetByIdAsync(dto.BookingId.Value)
                    ?? throw new Exception("Không tìm thấy Booking.");

                if (booking.Status is not ("Pending" or "Confirmed"))
                    throw new Exception("Booking không hợp lệ để bắt đầu sạc.");

                portId = booking.PortId;
                vehicleId = booking.VehicleId;

                booking.Status = "InProgress";
                await _bookingRepo.UpdateAsync(booking);
            }
            else
            {
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

            var session = new ChargingSession
            {
                BookingId = dto.BookingId,
                CustomerId = dto.CustomerId > 0 ? dto.CustomerId : null,
                CompanyId = dto.CompanyId > 0 ? dto.CompanyId : vehicle.CompanyId,
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
            var session = await _sessionRepo.GetByIdAsync(dto.ChargingSessionId)
                ?? throw new Exception("Không tìm thấy phiên sạc.");

            var rule = await _pricingRepo.GetByIdAsync(session.PricingRuleId)
                ?? throw new Exception("Không tìm thấy PricingRule.");

            var vehicle = await _vehicleRepo.GetByIdAsync(session.VehicleId)
                ?? throw new Exception("Không tìm thấy xe cho phiên sạc này.");

            if (vehicle.BatteryCapacity == null || vehicle.BatteryCapacity <= 0)
                throw new Exception("Dung lượng pin của xe không hợp lệ.");

            int startSoc = session.StartSoc ?? 50;
            int endSoc = dto.EndSoc ?? new Random().Next(startSoc + 10, 101);
            if (endSoc <= startSoc)
                throw new Exception("SOC kết thúc phải lớn hơn SOC bắt đầu.");

            session.EnergyKwh = Math.Round(vehicle.BatteryCapacity.Value * (endSoc - startSoc) / 100M, 2);
            session.EndSoc = endSoc;
            session.EndedAt = DateTime.Now;
            session.DurationMin = (int)(DateTime.Now - session.StartedAt!.Value).TotalMinutes;
            session.IdleMin = _rand.Next(0, 10);

            var activeSub = await _subscriptionRepo.GetActiveByCustomerOrCompanyAsync(
                session.CustomerId,
                session.CompanyId
            );

            decimal pricePerKwh = rule.PricePerKwh;
            decimal idleFeePerMin = rule.IdleFeePerMin;
            int freeIdle = activeSub?.SubscriptionPlan?.FreeIdleMinutes ?? 0;
            decimal discountPercent = activeSub?.SubscriptionPlan?.DiscountPercent ?? 0;

            int actualIdle = session.IdleMin ?? 0;
            int chargeableIdle = Math.Max(actualIdle - freeIdle, 0);

            decimal subtotal = (session.EnergyKwh ?? 0M) * pricePerKwh + chargeableIdle * idleFeePerMin;
            if (discountPercent > 0)
                subtotal -= subtotal * (discountPercent / 100M);

            session.Subtotal = Math.Round(subtotal, 2);
            session.Tax = Math.Round(session.Subtotal.Value * 0.1M, 2);
            session.Total = session.Subtotal + session.Tax;
            session.Status = "Completed";
            session.UpdatedAt = DateTime.Now;

            await _sessionRepo.UpdateAsync(session);

            // 🔹 Giải phóng port
            var port = await _portRepo.GetByIdAsync(session.PortId);
            if (port != null)
            {
                port.Status = "Available";
                await _portRepo.UpdateAsync(port);
            }

            // 🔹 Cập nhật Booking
            if (session.BookingId.HasValue)
            {
                var booking = await _bookingRepo.GetByIdAsync(session.BookingId.Value);
                if (booking != null)
                {
                    booking.Status = "Completed";
                    await _bookingRepo.UpdateAsync(booking);
                }
            }

            // 🔹 Tạo hoặc lấy invoice phù hợp (⚠️ sửa lỗi FK)
            var now = DateTime.Now;
            var invoice = await _invoiceRepo.GetOrCreateMonthlyInvoiceAsync(
                session.CustomerId,
                session.CompanyId,
                now.Month,
                now.Year
            );

            if (invoice.Status == "Paid")
            {
                invoice = new Invoice
                {
                    CustomerId = session.CustomerId,
                    CompanyId = session.CompanyId,
                    BillingMonth = now.Month,
                    BillingYear = now.Year,
                    CreatedAt = DateTime.Now,
                    Status = "Unpaid",
                    IsMonthlyInvoice = true
                };
                await _invoiceRepo.AddAsync(invoice);
            }

            if (activeSub != null)
                invoice.SubscriptionId = activeSub.SubscriptionId;

            invoice.ChargingSessions.Add(session);
            invoice.Total = (invoice.Total ?? 0M) + session.Total;
            invoice.UpdatedAt = DateTime.Now;

            await _invoiceRepo.SaveAsync();
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
