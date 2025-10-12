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

        // =======================
        // 🔹 Lấy danh sách (phân trang)
        // =======================
        public async Task<PagedResult<BookingDtos.ListItem>> GetAllAsync(BookingDtos.Query q)
        {
            var query = _repo.GetAll();

            // Filter
            if (q.CustomerId.HasValue)
                query = query.Where(x => x.CustomerId == q.CustomerId);

            if (q.VehicleId.HasValue)
                query = query.Where(x => x.VehicleId == q.VehicleId);

            if (q.PortId.HasValue)
                query = query.Where(x => x.PortId == q.PortId);

            if (!string.IsNullOrEmpty(q.Status))
                query = query.Where(x => x.Status == q.Status);

            if (!string.IsNullOrEmpty(q.Search))
                query = query.Where(x => x.Status.Contains(q.Search) ||
                                         x.Price.ToString().Contains(q.Search));

            // Sort
            bool desc = q.SortDir?.ToLower() == "desc";
            query = (q.SortBy ?? "CreatedAt").ToLower() switch
            {
                "price" => desc ? query.OrderByDescending(x => x.Price) : query.OrderBy(x => x.Price),
                "starttime" => desc ? query.OrderByDescending(x => x.StartTime) : query.OrderBy(x => x.StartTime),
                _ => desc ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt)
            };

            // Paging
            var total = await query.LongCountAsync();
            var items = await query.Skip((q.Page - 1) * q.PageSize)
                                   .Take(q.PageSize)
                                   .Select(x => new BookingDtos.ListItem
                                   {
                                       BookingId = x.BookingId,
                                       CustomerId = x.CustomerId,
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

        // =======================
        // 🔹 Lấy chi tiết
        // =======================
        public async Task<BookingDtos.Detail?> GetByIdAsync(int id)
        {
            var b = await _repo.GetByIdAsync(id);
            if (b == null) return null;

            return new BookingDtos.Detail
            {
                BookingId = b.BookingId,
                CustomerId = b.CustomerId,
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

        // =======================
        // 🔹 Tạo mới Booking
        // =======================
        public async Task<string> CreateAsync(BookingDtos.Create dto)
        {
            if (dto.StartTime >= dto.EndTime)
                return "Thời gian bắt đầu phải nhỏ hơn thời gian kết thúc.";

            // ⚠️ Kiểm tra đặt trước tối thiểu 1 tiếng
            if (dto.StartTime < DateTime.Now.AddHours(1))
                return "Bạn cần đặt lịch trước ít nhất 1 tiếng trước khi sạc.";

            // 🔸 Kiểm tra xe hợp lệ
            var vehicle = await _db.Vehicles.FindAsync(dto.VehicleId);
            if (vehicle == null)
                return "Không tìm thấy xe.";

            // 🔸 Kiểm tra trùng giờ đặt
            var overlap = await _db.Bookings.AnyAsync(x =>
                x.PortId == dto.PortId &&
                dto.StartTime < x.EndTime &&
                dto.EndTime > x.StartTime);
            if (overlap)
                return "Khoảng thời gian này đã có đặt lịch tại cổng sạc này.";

            // 🔹 Tính giá theo loại xe
            var price = CalculatePrice(dto.StartTime, dto.EndTime, vehicle.VehicleType);

            var booking = new Booking
            {
                CustomerId = dto.CustomerId,
                VehicleId = dto.VehicleId,
                PortId = dto.PortId,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                Price = price,
                Status = dto.Status,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            await _repo.AddAsync(booking);
            await _repo.SaveAsync();

            return $"Tạo đặt lịch thành công! Giá: {price:N0} VNĐ";
        }

        // =======================
        // 🔹 Cập nhật Booking
        // =======================
        public async Task<string> UpdateAsync(int id, BookingDtos.Update dto)
        {
            var b = await _repo.GetByIdAsync(id);
            if (b == null)
                return "Không tìm thấy đặt lịch.";

            if (dto.StartTime >= dto.EndTime)
                return "Thời gian bắt đầu phải nhỏ hơn thời gian kết thúc.";

            if (dto.StartTime < DateTime.Now.AddHours(1))
                return "Bạn chỉ có thể cập nhật đặt lịch nếu thời gian bắt đầu còn ít nhất 1 tiếng.";

            // 🔸 Kiểm tra trùng giờ khác
            var overlap = await _db.Bookings.AnyAsync(x =>
                x.PortId == dto.PortId &&
                x.BookingId != id &&
                dto.StartTime < x.EndTime &&
                dto.EndTime > x.StartTime);
            if (overlap)
                return "Khoảng thời gian này đã được đặt trước.";

            // 🔸 Lấy thông tin xe
            var vehicle = await _db.Vehicles.FindAsync(dto.VehicleId);
            if (vehicle == null)
                return "Không tìm thấy xe.";

            // 🔹 Tính lại giá
            var price = CalculatePrice(dto.StartTime, dto.EndTime, vehicle.VehicleType);

            b.VehicleId = dto.VehicleId;
            b.PortId = dto.PortId;
            b.StartTime = dto.StartTime;
            b.EndTime = dto.EndTime;
            b.Price = price;
            b.Status = dto.Status;
            b.UpdatedAt = DateTime.Now;

            await _repo.UpdateAsync(b);
            await _repo.SaveAsync();

            return $"Cập nhật đặt lịch thành công! Giá mới: {price:N0} VNĐ";
        }

        // =======================
        // 🔹 Xóa Booking
        // =======================
        public async Task<string> DeleteAsync(int id)
        {
            var b = await _repo.GetByIdAsync(id);
            if (b == null)
                return "Không tìm thấy đặt lịch.";

            await _repo.DeleteAsync(b);
            await _repo.SaveAsync();

            return "Xóa đặt lịch thành công!";
        }

        // =======================
        // 🔹 Tính giá theo loại xe
        // =======================
        private decimal CalculatePrice(DateTime? start, DateTime? end, string vehicleType)
        {
            if (start == null || end == null)
                return 0;

            var duration = end.Value - start.Value;
            var hours = Math.Ceiling(duration.TotalHours);

            decimal rate = 20000m; // mặc định xe máy

            if (!string.IsNullOrEmpty(vehicleType))
            {
                var type = vehicleType.Trim().ToLower();

                // 🔹 Kiểm tra chuẩn xác
                if (type == "car")
                    rate = 40000m;
                else if (type == "motorbike")
                    rate = 20000m;
                else
                    rate = 20000m; // fallback nếu nhập sai
            }

            return (decimal)hours * rate;
        }

    }
}
