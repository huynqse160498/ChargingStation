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
        // 🔹 Helper: xác định loại xe & dung lượng pin cho khách vãng lai
        // ============================================================
        private (string type, decimal battery) DetermineVehicleType(string? manualType, string connectorType, decimal powerKw)
        {
            if (!string.IsNullOrWhiteSpace(manualType))
            {
                string t = manualType.Trim().ToLower();
                if (t.Contains("bike") || t.Contains("xe máy")) return ("Motorbike", 3M);
                if (t.Contains("car") || t.Contains("ô tô")) return ("Car", 40M);
            }

            string c = connectorType.ToLower();
            if (c.Contains("scooter") || c.Contains("2-pin")) return ("Motorbike", 3M);
            if (c.Contains("type2")) return ("Car", powerKw >= 22 ? 60M : 40M);
            if (c.Contains("ccs2") || c.Contains("chademo")) return ("Car", 60M);
            return powerKw <= 7 ? ("Motorbike", 3M) : ("Car", 40M);
        }

        // ============================================================
        // 🔹 Bắt đầu phiên sạc (Customer / Company)
        // ============================================================
        public async Task<ChargingSession> StartSessionAsync(ChargingSessionCreateDto dto)
        {
            int portId;
            int vehicleId;

            // 1️⃣ Nếu có Booking
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

            // 3️⃣ Lấy thông tin Charger và Vehicle
            var portEntity = await _portRepo.GetByIdAsync(portId)
                ?? throw new Exception("Không tìm thấy Port.");
            var charger = portEntity.Charger
                ?? throw new Exception("Không tìm thấy Charger của port.");
            var vehicle = await _vehicleRepo.GetByIdAsync(vehicleId)
                ?? throw new Exception("Không tìm thấy xe.");

            string timeRange = GetCurrentTimeRange();

            // 4️⃣ Tìm PricingRule phù hợp
            var rule = await _pricingRepo.GetAll()
                .Where(x =>
                    x.ChargerType == charger.Type &&
                    x.PowerKw == charger.PowerKw &&
                    x.TimeRange == timeRange &&
                    x.Status == "Active")
                .FirstOrDefaultAsync()
                ?? throw new Exception($"Không có PricingRule cho {charger.Type} - {charger.PowerKw}kW ({timeRange}).");

            // 5️⃣ Xác định Customer/Company
            int? customerId = dto.CustomerId > 0 ? dto.CustomerId : null;
            int? companyId = dto.CustomerId > 0 ? null : (dto.CompanyId > 0 ? dto.CompanyId : vehicle.CompanyId);

            // 6️⃣ Tạo session mới
            var session = new ChargingSession
            {
                BookingId = dto.BookingId,
                CustomerId = customerId,
                CompanyId = companyId,
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
        // 🔹 Kết thúc phiên sạc (Customer / Company)
        // ============================================================
        public async Task<ChargingSession> EndSessionAsync(ChargingSessionEndDto dto)
        {
            var session = await _sessionRepo.GetByIdAsync(dto.ChargingSessionId)
                ?? throw new Exception("Không tìm thấy phiên sạc.");

            if (session.Status == "Completed")
                throw new Exception("Phiên sạc này đã kết thúc trước đó.");

            var rule = await _pricingRepo.GetByIdAsync(session.PricingRuleId)
                ?? throw new Exception("Không tìm thấy PricingRule.");

            var vehicle = await _vehicleRepo.GetByIdAsync(session.VehicleId)
                ?? throw new Exception("Không tìm thấy xe cho phiên sạc này.");

            if (vehicle.BatteryCapacity is null or <= 0)
                throw new Exception("Dung lượng pin của xe không hợp lệ.");

            int startSoc = session.StartSoc ?? 50;
            int endSoc = dto.EndSoc ?? new Random().Next(startSoc + 10, 101);
            if (endSoc <= startSoc)
                throw new Exception("SOC kết thúc phải lớn hơn SOC bắt đầu.");

            session.EndSoc = endSoc;
            session.EndedAt = DateTime.Now;
            session.EnergyKwh = Math.Round(vehicle.BatteryCapacity.Value * (endSoc - startSoc) / 100M, 2);
            session.DurationMin = (int)(session.EndedAt.Value - session.StartedAt!.Value).TotalMinutes;
            session.IdleMin = new Random().Next(0, 10);

            var activeSub = await _subscriptionRepo.GetActiveByCustomerOrCompanyAsync(session.CustomerId, session.CompanyId);

            decimal pricePerKwh = rule.PricePerKwh;
            decimal idleFeePerMin = rule.IdleFeePerMin;
            int freeIdle = activeSub?.SubscriptionPlan?.FreeIdleMinutes ?? 0;
            decimal discountPercent = activeSub?.SubscriptionPlan?.DiscountPercent ?? 0;

            int actualIdle = session.IdleMin ?? 0;
            int chargeableIdle = Math.Max(actualIdle - freeIdle, 0);

            decimal subtotalBeforeDiscount = (session.EnergyKwh ?? 0M) * pricePerKwh + chargeableIdle * idleFeePerMin;
            decimal subtotal = subtotalBeforeDiscount;

            if (discountPercent > 0)
                subtotal -= subtotal * (discountPercent / 100M);

            session.Subtotal = Math.Round(subtotal, 2);
            session.Tax = Math.Round(session.Subtotal.Value * 0.1M, 2);
            session.Total = session.Subtotal + session.Tax;
            session.Status = "Completed";
            session.UpdatedAt = DateTime.Now;

            await _sessionRepo.UpdateAsync(session);

            var port = await _portRepo.GetByIdAsync(session.PortId);
            if (port != null)
            {
                port.Status = "Available";
                await _portRepo.UpdateAsync(port);
            }

            if (session.BookingId.HasValue)
            {
                var booking = await _bookingRepo.GetByIdAsync(session.BookingId.Value);
                if (booking != null)
                {
                    booking.Status = "Completed";
                    await _bookingRepo.UpdateAsync(booking);
                }
            }

            var now = DateTime.Now;
            var invoice = await _invoiceRepo.GetOrCreateMonthlyInvoiceAsync(session.CustomerId, session.CompanyId, now.Month, now.Year);

            if (invoice.Status == "Paid")
            {
                invoice = new Invoice
                {
                    CustomerId = session.CustomerId,
                    CompanyId = session.CompanyId,
                    BillingMonth = now.Month,
                    BillingYear = now.Year,
                    Status = "Unpaid",
                    IsMonthlyInvoice = true,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                await _invoiceRepo.AddAsync(invoice);
            }

            if (activeSub != null)
            {
                invoice.SubscriptionId = activeSub.SubscriptionId;
                await _invoiceRepo.UpdateAsync(invoice);
            }

            invoice.ChargingSessions ??= new List<ChargingSession>();
            invoice.ChargingSessions.Add(session);
            invoice.Total = (invoice.Total ?? 0M) + session.Total;
            invoice.UpdatedAt = DateTime.Now;

            await _invoiceRepo.SaveAsync();

            session.InvoiceId = invoice.InvoiceId;
            await _sessionRepo.UpdateAsync(session);

            return session;
        }

        // ============================================================
        // 🔹 Bắt đầu phiên sạc cho khách vãng lai
        // ============================================================
        public async Task<ChargingSession> StartGuestSessionAsync(GuestChargingStartDto dto)
        {
            // 1️⃣ Kiểm tra trụ sạc hợp lệ
            var port = await _portRepo.Query()
                .Include(p => p.Charger)
                .FirstOrDefaultAsync(p => p.Code == dto.PortCode)
                ?? throw new Exception("Không tìm thấy trụ sạc với mã này.");

            if (port.Status != "Available")
                throw new Exception("Trụ sạc đang bận hoặc không khả dụng.");

            var charger = port.Charger
                ?? throw new Exception("Không tìm thấy thông tin bộ sạc (Charger) của trụ này.");

            // ⚡ 2️⃣ Xác định loại xe dựa vào ConnectorType và MaxPowerKw
            var (vehicleType, batteryCapacity) = DetermineVehicleType(
                dto.VehicleType,
                port.ConnectorType,
                port.MaxPowerKw ?? charger.PowerKw ?? 0M
            );

            // 🚗 3️⃣ Tạo bản ghi Vehicle tạm cho khách vãng lai
            var vehicle = new Vehicle
            {
                LicensePlate = dto.LicensePlate,
                VehicleType = vehicleType,
                BatteryCapacity = batteryCapacity,
                ConnectorType = port.ConnectorType,
                ManufactureYear = DateTime.Now.Year,
                CreatedAt = DateTime.Now
            };
            await _vehicleRepo.AddAsync(vehicle);

            // 🧾 4️⃣ Tìm PricingRule phù hợp
            string timeRange = GetCurrentTimeRange();
            var rule = await _pricingRepo.GetAll()
                .Where(r =>
                    r.ChargerType == charger.Type &&
                    r.PowerKw == (charger.PowerKw ?? 0M) &&
                    r.TimeRange == timeRange &&
                    r.Status == "Active")
                .FirstOrDefaultAsync()
                ?? throw new Exception($"Không tìm thấy PricingRule phù hợp cho {charger.Type} - {charger.PowerKw}kW ({timeRange}).");

            // 🔋 5️⃣ Random SOC ban đầu
            int startSoc = _rand.Next(20, 60);

            // 🕐 6️⃣ Tạo ChargingSession
            var session = new ChargingSession
            {
                VehicleId = vehicle.VehicleId,
                PortId = port.PortId,
                PricingRuleId = rule.PricingRuleId,
                StartSoc = startSoc,
                StartedAt = DateTime.Now,
                CreatedAt = DateTime.Now,
                Status = "Charging"
            };

            // ⚙️ 7️⃣ Cập nhật trạng thái trụ
            port.Status = "InUse";
            port.UpdatedAt = DateTime.Now;
            await _portRepo.UpdateAsync(port);

            // 💾 8️⃣ Lưu phiên sạc
            await _sessionRepo.AddAsync(session);

            return session;
        }



        // ============================================================
        // 🔹 Kết thúc phiên sạc cho khách vãng lai
        // ============================================================
        public async Task<ChargingSession> EndGuestSessionAsync(GuestChargingEndDto dto)
        {
            // 1️⃣ Lấy thông tin phiên sạc đầy đủ
            var session = await _sessionRepo.Query()
                .Include(s => s.PricingRule)
                .Include(s => s.Port)
                    .ThenInclude(p => p.Charger)
                .Include(s => s.Vehicle)
                .FirstOrDefaultAsync(s => s.ChargingSessionId == dto.ChargingSessionId)
                ?? throw new Exception("Không tìm thấy phiên sạc.");

            if (session.Status == "Completed")
                throw new Exception("Phiên sạc này đã kết thúc trước đó.");

            var rule = session.PricingRule
                ?? throw new Exception("Không tìm thấy PricingRule cho phiên sạc.");
            var vehicle = session.Vehicle
                ?? throw new Exception("Không tìm thấy thông tin xe.");
            var port = session.Port
                ?? throw new Exception("Không tìm thấy trụ sạc.");

            // 2️⃣ Tính toán năng lượng & tiền
            int startSoc = session.StartSoc ?? 40;
            int endSoc = dto.EndSoc;

            if (endSoc <= startSoc)
                throw new Exception("Mức SOC kết thúc phải lớn hơn mức SOC bắt đầu.");

            // 🔋 Tính điện năng tiêu thụ (kWh)
            decimal energyKwh = Math.Round(vehicle.BatteryCapacity!.Value * (endSoc - startSoc) / 100M, 2);

            // 💰 Tính tiền
            decimal subtotal = Math.Round(energyKwh * rule.PricePerKwh, 2);
            decimal tax = Math.Round(subtotal * 0.1M, 2);
            decimal total = subtotal + tax;

            // 3️⃣ Cập nhật thông tin phiên sạc
            session.EndSoc = endSoc;
            session.EnergyKwh = energyKwh;
            session.Subtotal = subtotal;
            session.Tax = tax;
            session.Total = total;
            session.Status = "Completed";
            session.EndedAt = DateTime.Now;
            session.UpdatedAt = DateTime.Now;

            await _sessionRepo.UpdateAsync(session);

            // 4️⃣ Giải phóng trụ sạc
            port.Status = "Available";
            port.UpdatedAt = DateTime.Now;
            await _portRepo.UpdateAsync(port);

            // ✅ 5️⃣ Trả kết quả cho FE
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
