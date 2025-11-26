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
        // 🔹 Helper: xác định khung giờ hiện tại
        // ============================================================
        private string GetCurrentTimeRange()
        {
            int hour = DateTime.Now.Hour;
            if (hour >= 22 || hour < 6) return "Low";
            if (hour >= 6 && hour < 17) return "Normal";
            return "Peak";
        }

        // ============================================================
        // 🔹 Helper: xác định loại xe & dung lượng pin
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

            // 1️⃣ Có booking
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
                // 2️⃣ Không có booking
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

            // 3️⃣ Kiểm tra connector type tương thích
            var portEntity = await _portRepo.GetByIdAsync(portId)
                ?? throw new Exception("Không tìm thấy Port.");
            var charger = portEntity.Charger
                ?? throw new Exception("Không tìm thấy Charger của port.");
            var vehicle = await _vehicleRepo.GetByIdAsync(vehicleId)
                ?? throw new Exception("Không tìm thấy xe.");

            if (!string.Equals(vehicle.ConnectorType, portEntity.ConnectorType, StringComparison.OrdinalIgnoreCase))
                throw new Exception($"Xe ({vehicle.ConnectorType}) không tương thích với cổng sạc ({portEntity.ConnectorType}).");

            // 4️⃣ PricingRule
            string timeRange = GetCurrentTimeRange();
            var rule = await _pricingRepo.GetAll()
                .Where(x => x.ChargerType == charger.Type && x.PowerKw == charger.PowerKw && x.TimeRange == timeRange && x.Status == "Active")
                .FirstOrDefaultAsync()
                ?? throw new Exception($"Không có PricingRule cho {charger.Type} - {charger.PowerKw}kW ({timeRange}).");

            // 5️⃣ Xác định Customer/Company
            int? customerId = dto.CustomerId > 0 ? dto.CustomerId : null;
            int? companyId = dto.CustomerId > 0 ? null : (dto.CompanyId > 0 ? dto.CompanyId : vehicle.CompanyId);

            // 6️⃣ Tạo session
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
                ?? throw new Exception("Không tìm thấy xe.");

            if (vehicle.BatteryCapacity is null or <= 0)
                throw new Exception("Dung lượng pin của xe không hợp lệ.");

            // SOC
            int startSoc = session.StartSoc ?? 50;
            int endSoc = dto.EndSoc ?? _rand.Next(startSoc + 10, 101);
            if (endSoc <= startSoc)
                throw new Exception("SOC kết thúc phải lớn hơn SOC bắt đầu.");

            // =========================
            // ⚡ Tính toán chi phí
            // =========================
            session.EndSoc = endSoc;
            session.EndedAt = DateTime.Now;

            session.EnergyKwh = Math.Round(vehicle.BatteryCapacity.Value * (endSoc - startSoc) / 100M, 2);
            session.DurationMin = (int)(session.EndedAt.Value - session.StartedAt!.Value).TotalMinutes;
            session.IdleMin = dto.IdleMin ?? 0;

            var activeSub = await _subscriptionRepo
                .GetActiveByCustomerOrCompanyAsync(session.CustomerId, session.CompanyId);

            // Subtotal
            decimal subtotal = (session.EnergyKwh ?? 0) * rule.PricePerKwh;

            int freeIdle = activeSub?.SubscriptionPlan?.FreeIdleMinutes ?? 0;
            int chargeableIdle = Math.Max(session.IdleMin.Value - freeIdle, 0);
            subtotal += chargeableIdle * rule.IdleFeePerMin;

            decimal discountPercent = activeSub?.SubscriptionPlan?.DiscountPercent ?? 0;
            if (discountPercent > 0)
                subtotal -= subtotal * (discountPercent / 100M);

            session.Subtotal = Math.Round(subtotal, 2);
            session.Tax = Math.Round(session.Subtotal.Value * 0.1M, 2);
            session.Total = session.Subtotal + session.Tax;
            session.Status = "Completed";
            session.UpdatedAt = DateTime.Now;

            await _sessionRepo.UpdateAsync(session);

            // ============================================================
            // 🔓 Giải phóng Port
            // ============================================================
            var port = await _portRepo.GetByIdAsync(session.PortId);
            if (port != null)
            {
                port.Status = "Available";
                await _portRepo.UpdateAsync(port);
            }

            // ============================================================
            // 📌 Booking
            // ============================================================
            if (session.BookingId.HasValue)
            {
                var booking = await _bookingRepo.GetByIdAsync(session.BookingId.Value);
                if (booking != null)
                {
                    booking.Status = "Completed";
                    await _bookingRepo.UpdateAsync(booking);
                }
            }

            // ============================================================
            // 🧾 HÓA ĐƠN — LOGIC ĐÃ FIX CHUẨN
            // ============================================================
            if (session.CustomerId != null || session.CompanyId != null)
            {
                var now = DateTime.UtcNow.AddHours(7);

                var invoice = await _invoiceRepo.GetMonthlyInvoiceAsync(
                    session.CustomerId,
                    session.CompanyId,
                    now.Month,
                    now.Year
                );

             
                if (invoice == null || invoice.Status == "Paid")
                {
                    invoice = new Invoice
                    {
                        CustomerId = session.CustomerId,
                        CompanyId = session.CompanyId,
                        BillingMonth = now.Month,
                        BillingYear = now.Year,
                        Status = "Unpaid",
                        IsMonthlyInvoice = true,
                        CreatedAt = now,
                        UpdatedAt = now,
                        DueDate = now.AddMonths(1)
                    };

                    await _invoiceRepo.AddAsync(invoice);
                }

                // Gắn Subscription
                if (activeSub != null)
                {
                    invoice.SubscriptionId = activeSub.SubscriptionId;
                }

                // Thêm session vào invoice
                invoice.ChargingSessions ??= new List<ChargingSession>();
                invoice.ChargingSessions.Add(session);

                // Recalculate invoice tổng
                invoice.Subtotal = invoice.ChargingSessions.Sum(s => s.Subtotal ?? 0);
                invoice.Tax = Math.Round(invoice.Subtotal.Value * 0.1M, 2);
                invoice.Total = invoice.Subtotal + invoice.Tax + (invoice.SubscriptionAdjustment ?? 0);
                invoice.UpdatedAt = now;

                await _invoiceRepo.UpdateAsync(invoice);

                // Gán invoiceId vào session
                session.InvoiceId = invoice.InvoiceId;
                await _sessionRepo.UpdateAsync(session);
            }

            return session;
        }



        // ============================================================
        // 🔹 Bắt đầu phiên sạc cho khách vãng lai
        // ============================================================
        public async Task<ChargingSession> StartGuestSessionAsync(GuestChargingStartDto dto)
        {
            var port = await _portRepo.Query()
                .Include(p => p.Charger)
                .FirstOrDefaultAsync(p => p.Code == dto.PortCode)
                ?? throw new Exception("Không tìm thấy trụ sạc với mã này.");

            if (port.Status != "Available")
                throw new Exception("Trụ sạc đang bận hoặc không khả dụng.");

            var charger = port.Charger ?? throw new Exception("Không tìm thấy thông tin bộ sạc.");

            var connector = string.IsNullOrEmpty(dto.ConnectorType) ? port.ConnectorType : dto.ConnectorType;

            if (!string.IsNullOrEmpty(dto.ConnectorType) &&
                !string.Equals(dto.ConnectorType, port.ConnectorType, StringComparison.OrdinalIgnoreCase))
                throw new Exception($"Đầu nối '{dto.ConnectorType}' không tương thích với trụ '{port.ConnectorType}'.");

            var (vehicleType, batteryCapacity) = DetermineVehicleType(dto.VehicleType, connector, port.MaxPowerKw ?? charger.PowerKw ?? 0M);

            var vehicle = new Vehicle
            {
                LicensePlate = dto.LicensePlate,
                VehicleType = vehicleType,
                BatteryCapacity = batteryCapacity,
                ConnectorType = connector,
                ManufactureYear = DateTime.Now.Year,
                CreatedAt = DateTime.Now
            };
            await _vehicleRepo.AddAsync(vehicle);

            string timeRange = GetCurrentTimeRange();
            var rule = await _pricingRepo.GetAll()
                .Where(r => r.ChargerType == charger.Type && r.PowerKw == (charger.PowerKw ?? 0M) && r.TimeRange == timeRange && r.Status == "Active")
                .FirstOrDefaultAsync()
                ?? throw new Exception($"Không tìm thấy PricingRule phù hợp cho {charger.Type} - {charger.PowerKw}kW ({timeRange}).");

            int startSoc = _rand.Next(20, 60);

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

            port.Status = "InUse";
            port.UpdatedAt = DateTime.Now;
            await _portRepo.UpdateAsync(port);

            await _sessionRepo.AddAsync(session);
            return session;
        }

        // ============================================================
        // 🔹 Kết thúc phiên sạc cho khách vãng lai
        // ============================================================
        public async Task<ChargingSession> EndGuestSessionAsync(GuestChargingEndDto dto)
        {
            var session = await _sessionRepo.Query()
                .Include(s => s.PricingRule)
                .Include(s => s.Port).ThenInclude(p => p.Charger)
                .Include(s => s.Vehicle)
                .FirstOrDefaultAsync(s => s.ChargingSessionId == dto.ChargingSessionId)
                ?? throw new Exception("Không tìm thấy phiên sạc.");

            if (session.Status == "Completed")
                throw new Exception("Phiên sạc này đã kết thúc.");

            var rule = session.PricingRule ?? throw new Exception("Không tìm thấy PricingRule.");
            var vehicle = session.Vehicle ?? throw new Exception("Không tìm thấy xe.");
            var port = session.Port ?? throw new Exception("Không tìm thấy trụ sạc.");

            if (!string.Equals(vehicle.ConnectorType, port.ConnectorType, StringComparison.OrdinalIgnoreCase))
                throw new Exception($"Xe ({vehicle.ConnectorType}) không tương thích với cổng ({port.ConnectorType}).");

            int startSoc = session.StartSoc ?? 40;
            int endSoc = dto.EndSoc;
            if (endSoc <= startSoc)
                throw new Exception("SOC kết thúc phải lớn hơn SOC bắt đầu.");

            decimal energyKwh = Math.Round(vehicle.BatteryCapacity!.Value * (endSoc - startSoc) / 100M, 2);
            decimal subtotal = Math.Round(energyKwh * rule.PricePerKwh, 2);
            decimal tax = Math.Round(subtotal * 0.1M, 2);
            decimal total = subtotal + tax;

            session.EndSoc = endSoc;
            session.EnergyKwh = energyKwh;
            session.Subtotal = subtotal;
            session.Tax = tax;
            session.Total = total;
            session.Status = "Completed";
            session.EndedAt = DateTime.Now;
            session.UpdatedAt = DateTime.Now;

            await _sessionRepo.UpdateAsync(session);

            port.Status = "Available";
            port.UpdatedAt = DateTime.Now;
            await _portRepo.UpdateAsync(port);

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
