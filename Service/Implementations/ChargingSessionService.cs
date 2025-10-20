//using Repositories.DTOs;
//using Repositories.Interfaces;
//using Repositories.Models;
//using Services.Interfaces;

//namespace Services.Implementations
//{
//    public class ChargingSessionService : IChargingSessionService
//    {
//        private readonly IChargingSessionRepository _sessionRepo;
//        private readonly IPricingRuleRepository _pricingRepo;
//        private readonly IBookingRepository _bookingRepo;
//        private readonly IPortRepository _portRepo;
//        private readonly IVehicleRepository _vehicleRepo;
//        private readonly IInvoiceRepository _invoiceRepo;
//        private readonly Random _rand = new();

//        public ChargingSessionService(
//            IChargingSessionRepository sessionRepo,
//            IPricingRuleRepository pricingRepo,
//            IBookingRepository bookingRepo,
//            IPortRepository portRepo,
//            IVehicleRepository vehicleRepo,
//            IInvoiceRepository invoiceRepo)
//        {
//            _sessionRepo = sessionRepo;
//            _pricingRepo = pricingRepo;
//            _bookingRepo = bookingRepo;
//            _portRepo = portRepo;
//            _vehicleRepo = vehicleRepo;
//            _invoiceRepo = invoiceRepo;
//        }

//        // 🔹 Xác định khung giờ hiện tại
//        private string GetCurrentTimeRange()
//        {
//            int hour = DateTime.Now.Hour;
//            if (hour >= 22 || hour < 6) return "Low";      // Giờ thấp điểm
//            if (hour >= 6 && hour < 17) return "Normal";   // Giờ bình thường
//            return "Peak";                                 // Giờ cao điểm
//        }

//        // ✅ Bắt đầu phiên sạc
//        public async Task<ChargingSession> StartSessionAsync(ChargingSessionCreateDto dto)
//        {
//            int portId;
//            int vehicleId;
//            int customerId = dto.CustomerId;

//            // ====== 1️⃣ Nếu có Booking ======
//            if (dto.BookingId.HasValue)
//            {
//                var booking = await _bookingRepo.GetByIdAsync(dto.BookingId.Value)
//                    ?? throw new Exception("Không tìm thấy Booking.");

//                if (booking.CustomerId != dto.CustomerId)
//                    throw new Exception("Booking không thuộc khách hàng này.");

//                if (booking.Status != "Confirmed" && booking.Status != "Pending")
//                    throw new Exception("Booking không hợp lệ hoặc đã hoàn thành.");

//                portId = booking.PortId;
//                vehicleId = booking.VehicleId;

//                booking.Status = "InProgress";
//                await _bookingRepo.UpdateAsync(booking);
//            }
//            else
//            {
//                // ====== 2️⃣ Nếu KHÔNG có Booking ======
//                if (!dto.PortId.HasValue)
//                    throw new Exception("Phải chọn PortId khi không có Booking.");

//                var port = await _portRepo.GetByIdAsync(dto.PortId.Value)
//                    ?? throw new Exception("Không tìm thấy Port.");

//                if (port.Status != "Available")
//                    throw new Exception("Cổng sạc không khả dụng.");

//                port.Status = "InUse";
//                await _portRepo.UpdateAsync(port);

//                portId = port.PortId;
//                vehicleId = dto.VehicleId;
//            }

//            // ====== 3️⃣ Tự động xác định PricingRule ======
//            var vehicle = await _vehicleRepo.GetByIdAsync(vehicleId)
//                ?? throw new Exception("Không tìm thấy xe.");

//            string vehicleType = vehicle.VehicleType;
//            string timeRange = GetCurrentTimeRange();

//            var rule = await _pricingRepo.GetActiveRuleAsync(vehicleType, timeRange)
//                            ?? throw new Exception($"Không tìm thấy giá cho {vehicleType} ({timeRange}).");

//            // ====== 4️⃣ Tạo mới phiên sạc ======
//            var session = new ChargingSession
//            {
//                BookingId = dto.BookingId ?? 0, // Nếu không có booking thì gán 0 (hoặc null nếu bạn sửa model)
//                CustomerId = customerId,
//                VehicleId = vehicleId,
//                PortId = portId,
//                PricingRuleId = rule.PricingRuleId,
//                StartSoc = _rand.Next(20, 80), // SOC ban đầu ngẫu nhiên
//                StartedAt = DateTime.Now,
//                Status = "Charging"
//            };

//            await _sessionRepo.AddAsync(session);
//            return session;
//        }

//        public async Task<ChargingSession> EndSessionAsync(ChargingSessionEndDto dto)
//        {
//            try
//            {
//                var session = await _sessionRepo.GetByIdAsync(dto.ChargingSessionId)
//                    ?? throw new Exception("Không tìm thấy phiên sạc.");

//                var rule = await _pricingRepo.GetByIdAsync(session.PricingRuleId)
//                    ?? throw new Exception("Không tìm thấy PricingRule.");

//                int endSoc = dto.EndSoc ?? new Random().Next((session.StartSoc ?? 50) + 10, 101);

//                session.EndSoc = endSoc;
//                session.EndedAt = DateTime.Now;
//                session.DurationMin = 45;
//                session.IdleMin = 5;
//                session.EnergyKwh = (endSoc - (session.StartSoc ?? 50)) * 0.4M;
//                session.Subtotal = session.EnergyKwh * rule.PricePerKwh + session.IdleMin * rule.IdleFeePerMin;
//                session.Tax = session.Subtotal * 0.1M;
//                session.Total = session.Subtotal + session.Tax;
//                session.Status = "Completed";

//                await _sessionRepo.UpdateAsync(session);

//                // Giải phóng cổng
//                var port = await _portRepo.GetByIdAsync(session.PortId);
//                if (port != null)
//                {
//                    port.Status = "Available";
//                    await _portRepo.UpdateAsync(port);
//                }

//                // Gắn hóa đơn
//                var now = DateTime.Now;
//                var invoice = await _invoiceRepo.GetOrCreateMonthlyInvoiceAsync(session.CustomerId, now.Month, now.Year);
//                invoice.ChargingSessions.Add(session);
//                invoice.Total = (invoice.Total ?? 0) + session.Total;
//                await _invoiceRepo.SaveAsync();

//                return session;
//            }
//            catch (Exception ex)
//            {
//                // ⚠️ Ghi log Inner Exception để xem lỗi thật
//                throw new Exception($"[DEBUG] Lỗi khi lưu phiên sạc: {ex.InnerException?.Message ?? ex.Message}");
//            }
        


//            //// ====== Cập nhật Booking (nếu có) ======
//            //if (session.BookingId.HasValue)
//            //{
//            //    var booking = await _bookingRepo.GetByIdAsync(session.BookingId.Value);
//            //    if (booking != null)
//            //    {
//            //        booking.Status = "Completed";
//            //        await _bookingRepo.UpdateAsync(booking);
//            //    }
//            //}

//            //return session;
//        }

//        // ✅ CRUD cơ bản
//        public async Task<List<ChargingSession>> GetAllAsync() => await _sessionRepo.GetAllAsync();
//        public async Task<ChargingSession?> GetByIdAsync(int id) => await _sessionRepo.GetByIdAsync(id);

//        public async Task DeleteAsync(int id)
//        {
//            var session = await _sessionRepo.GetByIdAsync(id)
//                ?? throw new Exception("Không tìm thấy phiên sạc.");
//            await _sessionRepo.DeleteAsync(session);
//        }
//    }
//}
