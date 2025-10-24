using Microsoft.EntityFrameworkCore;
using Repositories.DTOs;
using Repositories.Interfaces;
using Repositories.Models;
using Services.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Services.Implementations
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _repo;
        private readonly ChargeStationContext _db;

        public BookingService(IBookingRepository repo, ChargeStationContext db)
        {
            _repo = repo;
            _db = db;
        }

        // ===========================
        // 🔎 Danh sách + tìm kiếm
        // ===========================
        public async Task<PagedResult<BookingDtos.ListItem>> GetAllAsync(BookingDtos.Query q)
        {
            var query = _db.Bookings.AsNoTracking().AsQueryable();

            if (q.CustomerId.HasValue) query = query.Where(x => x.CustomerId == q.CustomerId);
            if (q.VehicleId.HasValue) query = query.Where(x => x.VehicleId == q.VehicleId);
            if (q.PortId.HasValue) query = query.Where(x => x.PortId == q.PortId);
            if (!string.IsNullOrEmpty(q.Status)) query = query.Where(x => x.Status == q.Status);
            if (!string.IsNullOrEmpty(q.Search))
                query = query.Where(x => x.Status.Contains(q.Search) || x.Price.ToString()!.Contains(q.Search));

            bool desc = (q.SortDir ?? "desc").Equals("desc", StringComparison.OrdinalIgnoreCase);
            query = (q.SortBy ?? "CreatedAt").ToLower() switch
            {
                "price" => desc ? query.OrderByDescending(x => x.Price) : query.OrderBy(x => x.Price),
                "starttime" => desc ? query.OrderByDescending(x => x.StartTime) : query.OrderBy(x => x.StartTime),
                _ => desc ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt),
            };

            var total = await query.LongCountAsync();
            var items = await query.Skip((q.Page - 1) * q.PageSize)
                                   .Take(q.PageSize)
                                   .Select(x => new BookingDtos.ListItem
                                   {
                                       BookingId = x.BookingId,
                                       CustomerId = x.CustomerId,
                                       CompanyId = x.CompanyId,
                                       VehicleId = x.VehicleId,
                                       PortId = x.PortId,
                                       StartTime = x.StartTime,
                                       EndTime = x.EndTime,
                                       Price = x.Price,
                                       Status = x.Status,
                                       CreatedAt = x.CreatedAt
                                   })
                                   .ToListAsync();

            return new PagedResult<BookingDtos.ListItem>
            {
                Page = q.Page,
                PageSize = q.PageSize,
                TotalItems = total,
                TotalPages = (int)Math.Ceiling(total / (double)q.PageSize),
                Items = items
            };
        }

        // ===========================
        // 🔎 Chi tiết
        // ===========================
        public async Task<BookingDtos.Detail?> GetByIdAsync(int id)
        {
            var b = await _db.Bookings.AsNoTracking().FirstOrDefaultAsync(x => x.BookingId == id);
            if (b == null) return null;

            return new BookingDtos.Detail
            {
                BookingId = b.BookingId,
                CustomerId = b.CustomerId,
                CompanyId = b.CompanyId,
                VehicleId = b.VehicleId,
                PortId = b.PortId,
                StartTime = b.StartTime,
                EndTime = b.EndTime,
                Price = b.Price,
                Status = b.Status,
                CreatedAt = b.CreatedAt,
                UpdatedAt = b.UpdatedAt
            };
        }

        // ===========================
        // ✅ Tạo mới (đặt cọc Port = Reserved)
        // ===========================
        public async Task<string> CreateAsync(BookingDtos.Create dto)
        {
            if (dto.StartTime >= dto.EndTime)
                return "⛔ Thời gian bắt đầu phải nhỏ hơn thời gian kết thúc.";

            if (dto.StartTime < DateTime.Now.AddHours(1))
                return "⏰ Bạn cần đặt lịch trước ít nhất 1 tiếng.";

            var vehicle = await _db.Vehicles.FindAsync(dto.VehicleId);
            if (vehicle == null)
                return "Không tìm thấy xe.";

            // Transaction để đảm bảo an toàn khi đặt cổng
            await using var tx = await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

            var port = await _db.Ports.FirstOrDefaultAsync(p => p.PortId == dto.PortId);
            if (port == null)
                return "Không tìm thấy cổng sạc.";

            // 🔌 Kiểm tra tương thích đầu nối
            if (!IsConnectorCompatible(vehicle.ConnectorType, port.ConnectorType))
                return $"Xe ({vehicle.ConnectorType}) không tương thích với cổng sạc ({port.ConnectorType}).";

            // 🚫 Nếu cổng đang bận
            if (string.Equals(port.Status, "InUse", StringComparison.OrdinalIgnoreCase))
                return "Cổng sạc hiện đang sử dụng.";

            // 🕐 Kiểm tra trùng lịch tại cổng này
            bool conflict = await _db.Bookings.AnyAsync(x =>
                x.PortId == dto.PortId &&
                x.Status != "Cancelled" &&
                dto.StartTime < x.EndTime &&
                dto.EndTime > x.StartTime);

            if (conflict)
                return "Khoảng thời gian này đã có đặt lịch tại cổng sạc này.";

            // ✅ Giữ chỗ cổng
            if (!string.Equals(port.Status, "InUse", StringComparison.OrdinalIgnoreCase))
                port.Status = "Reserved";

            var price = CalculatePrice(dto.StartTime, dto.EndTime, vehicle.VehicleType);

            // ==========================
            // 📦 Phân biệt người đặt (Customer hoặc Company)
            // ==========================
            int? customerId = dto.CustomerId;
            int? companyId = dto.CompanyId;

            if (customerId == null && companyId == null)
                return "Phải có CustomerId hoặc CompanyId để đặt lịch.";

            var booking = new Booking
            {
                CustomerId = customerId,
                CompanyId = companyId,
                VehicleId = dto.VehicleId,
                PortId = dto.PortId,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                Price = price,
                Status = "Pending",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            await _repo.AddAsync(booking);
            await _repo.SaveAsync();

            await tx.CommitAsync();

            string owner = customerId != null ? "Khách hàng" : "Công ty";
            return $"✅ {owner} đã đặt lịch thành công! Giá tạm tính: {price:N0} VNĐ";
        }


        // ===========================
        // ✏️ Cập nhật (giữ trạng thái Port hợp lệ)
        // ===========================
        public async Task<string> UpdateAsync(int id, BookingDtos.Update dto)
        {
            var booking = await _db.Bookings.FirstOrDefaultAsync(b => b.BookingId == id);
            if (booking == null)
                return "Không tìm thấy đặt lịch.";

            if (booking.Status is "InProgress" or "Completed")
                return "Không thể cập nhật khi đặt lịch đã bắt đầu hoặc đã hoàn tất.";

            if (dto.StartTime >= dto.EndTime)
                return "⛔ Thời gian bắt đầu phải nhỏ hơn thời gian kết thúc.";

            if (dto.StartTime < DateTime.Now.AddHours(1))
                return "⏰ Bạn chỉ có thể cập nhật đặt lịch nếu thời gian bắt đầu còn ít nhất 1 tiếng.";

            var vehicle = await _db.Vehicles.FindAsync(dto.VehicleId);
            if (vehicle == null)
                return "Không tìm thấy xe.";

            await using var tx = await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

            var newPort = await _db.Ports.FirstOrDefaultAsync(p => p.PortId == dto.PortId);
            if (newPort == null)
                return "Không tìm thấy cổng sạc.";

            // 🔌 Kiểm tra tương thích đầu nối
            if (!IsConnectorCompatible(vehicle.ConnectorType, newPort.ConnectorType))
                return $"Xe ({vehicle.ConnectorType}) không tương thích với cổng sạc ({newPort.ConnectorType}).";

            if (string.Equals(newPort.Status, "InUse", StringComparison.OrdinalIgnoreCase))
                return "Cổng sạc hiện đang sử dụng.";

            // 🕐 Kiểm tra trùng lịch
            bool conflict = await _db.Bookings.AnyAsync(x =>
                x.PortId == dto.PortId &&
                x.BookingId != id &&
                x.Status != "Cancelled" &&
                dto.StartTime < x.EndTime &&
                dto.EndTime > x.StartTime);

            if (conflict)
                return "Khoảng thời gian này đã được đặt trước.";

            // ✅ Nếu đổi Port, giải phóng port cũ (nếu cần)
            if (booking.PortId != dto.PortId)
            {
                var oldPort = await _db.Ports.FirstOrDefaultAsync(p => p.PortId == booking.PortId);
                if (oldPort != null && string.Equals(oldPort.Status, "Reserved", StringComparison.OrdinalIgnoreCase))
                {
                    bool stillOther = await _db.Bookings.AnyAsync(x =>
                        x.PortId == oldPort.PortId &&
                        x.BookingId != booking.BookingId &&
                        (x.Status == "Pending" || x.Status == "Confirmed"));
                    if (!stillOther)
                        oldPort.Status = "Available";
                }

                if (!string.Equals(newPort.Status, "InUse", StringComparison.OrdinalIgnoreCase))
                    newPort.Status = "Reserved";
            }

            // ========================
            // 📦 Cập nhật thông tin chung
            // ========================
            booking.PortId = dto.PortId;
            booking.VehicleId = dto.VehicleId;
            booking.StartTime = dto.StartTime;
            booking.EndTime = dto.EndTime;
            booking.Price = CalculatePrice(dto.StartTime, dto.EndTime, vehicle.VehicleType);
            booking.UpdatedAt = DateTime.Now;

            // ✅ Hỗ trợ cho cả Customer & Company
            booking.CustomerId = dto.CustomerId;
            booking.CompanyId = dto.CompanyId;

            await _repo.UpdateAsync(booking);
            await tx.CommitAsync();

            return $"✅ Cập nhật đặt lịch thành công! Giá tạm tính mới: {booking.Price:N0} VNĐ";
        }


        // ===========================
        // 🗑️ Xóa
        // ===========================
        public async Task<string> DeleteAsync(int id)
        {
            var booking = await _db.Bookings.FirstOrDefaultAsync(b => b.BookingId == id);
            if (booking == null) return "Không tìm thấy đặt lịch.";

            if (booking.Status is "InProgress")
                return "Không thể xóa khi đặt lịch đang diễn ra.";
            if (booking.Status is "Completed")
                return "Không thể xóa đặt lịch đã hoàn tất.";

            await using var tx = await _db.Database.BeginTransactionAsync();

            // Giải phóng port nếu đang Reserved và không còn ai xếp chỗ
            var port = await _db.Ports.FirstOrDefaultAsync(p => p.PortId == booking.PortId);
            if (port != null && string.Equals(port.Status, "Reserved", StringComparison.OrdinalIgnoreCase))
            {
                bool other = await _db.Bookings.AnyAsync(x =>
                     x.BookingId != booking.BookingId &&
                     x.PortId == port.PortId &&
                    (x.Status == "Pending" || x.Status == "Confirmed")
                  );
                if (!other)
                    port.Status = "Available";

            }

            await _repo.DeleteAsync(booking);
            await _repo.SaveAsync();

            await tx.CommitAsync();
            return "Xóa đặt lịch thành công!";
        }

        // ===========================
        // 🔁 Đổi trạng thái + đồng bộ Port
        // ===========================
        public async Task<string> ChangeStatusAsync(int id, string newStatus)
        {
            var booking = await _db.Bookings.FirstOrDefaultAsync(b => b.BookingId == id);
            if (booking == null) return "Không tìm thấy đặt lịch.";

            var valid = new[] { "Pending", "Confirmed", "InProgress", "Completed", "Cancelled" };
            if (!valid.Contains(newStatus)) return "Trạng thái không hợp lệ.";

            await using var tx = await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

            var port = await _db.Ports.FirstOrDefaultAsync(p => p.PortId == booking.PortId);
            if (port == null) return "Không tìm thấy cổng sạc.";

            // Quy tắc chuyển trạng thái + Port
            string old = booking.Status;

            // Không cho đổi từ Completed/Cancelled sang trạng thái khác
            if (old is "Completed" or "Cancelled")
                return "Đặt lịch đã kết thúc hoặc bị hủy, không thể thay đổi.";

            switch (newStatus)
            {
                case "Pending":
                    // quay về Pending -> Reserved
                    if (!string.Equals(port.Status, "InUse", StringComparison.OrdinalIgnoreCase))
                        port.Status = "Reserved";
                    break;

                case "Confirmed":
                    // giữ Reserved
                    if (!string.Equals(port.Status, "InUse", StringComparison.OrdinalIgnoreCase))
                        port.Status = "Reserved";
                    break;

                case "InProgress":
                    // chỉ cho phép khi gần StartTime (ví dụ: trong vòng -15..+60 phút)
                    var now = DateTime.Now;
                    if (booking.StartTime.HasValue &&
                        now < booking.StartTime.Value.AddMinutes(-15))
                        return "Chưa đến thời gian bắt đầu sạc.";
                    // chuyển cổng sang InUse
                    port.Status = "InUse";
                    break;

                case "Completed":
                    // kết thúc -> port Available (nếu không còn ai đặt)
                    port.Status = await HasOtherReservations(port.PortId, excludeBookingId: booking.BookingId)
                        ? "Reserved"
                        : "Available";
                    break;

                case "Cancelled":
                    // hủy -> port Available nếu không còn booking khác
                    port.Status = await HasOtherReservations(port.PortId, excludeBookingId: booking.BookingId)
                        ? "Reserved"
                        : "Available";
                    break;
            }

            booking.Status = newStatus;
            booking.UpdatedAt = DateTime.Now;

            await _repo.UpdateAsync(booking);
            await _repo.SaveAsync();

            await tx.CommitAsync();
            return $"Đã đổi trạng thái đặt lịch #{booking.BookingId} từ '{old}' → '{newStatus}'.";
        }

        // ===========================
        // 🕒 Dọn dẹp: auto-cancel nếu khách không đến
        // (Gọi từ background job mỗi 5-10 phút)
        // ===========================
        public async Task<int> AutoExpireNoShowAsync(int graceMinutes = 15)
        {
            var now = DateTime.Now;
            var toCancel = await _db.Bookings
                .Where(b => b.Status == "Pending" && b.StartTime < now.AddMinutes(-graceMinutes))
                .ToListAsync();

            foreach (var b in toCancel)
            {
                var port = await _db.Ports.FirstOrDefaultAsync(p => p.PortId == b.PortId);
                b.Status = "Cancelled";
                b.UpdatedAt = now;

                if (port != null)
                {
                    port.Status = await HasOtherReservations(port.PortId, excludeBookingId: b.BookingId)
                        ? "Reserved"
                        : "Available";
                }
            }

            return await _db.SaveChangesAsync();
        }

        // ===========================
        // 🔧 Helpers
        // ===========================
        private bool IsConnectorCompatible(string vehicleConnector, string portConnector)
        {
            if (string.IsNullOrWhiteSpace(vehicleConnector) || string.IsNullOrWhiteSpace(portConnector))
                return false;
            return vehicleConnector.Trim().Equals(portConnector.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        private async Task<bool> HasOtherReservations(int portId, int excludeBookingId)
        {
            return await _db.Bookings.AnyAsync(x =>
                x.PortId == portId &&
                x.BookingId != excludeBookingId &&
                (x.Status == "Pending" || x.Status == "Confirmed")
            );
        }


        private decimal CalculatePrice(DateTime? start, DateTime? end, string vehicleType)
        {
            if (start == null || end == null) return 0;
            var hours = Math.Ceiling((end.Value - start.Value).TotalHours);

            decimal rate = 20000m;
            if (!string.IsNullOrEmpty(vehicleType))
            {
                var t = vehicleType.Trim().ToLower();
                if (t == "car") rate = 40000m;
                else if (t == "motorbike") rate = 20000m;
            }
            return (decimal)hours * rate;
        }
    }
}
