using Microsoft.AspNetCore.Http;
using Repositories.DTOs.Chargers;
using Repositories.Interfaces;
using Repositories.Models;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services.Implementations
{
    public class ChargerService : IChargerService
    {
        private readonly IChargerRepository _repo;
        private readonly IS3Service _s3;


        // Status hợp lệ
        private const string ONLINE = "Online";
        private const string OFFLINE = "Offline";
        private const string OUT_OF_ORDER = "OutOfOrder";
            
        private static bool IsValidStatus(string? s)
            => s == ONLINE || s == OFFLINE || s == OUT_OF_ORDER;

        // chuẩn hoá: mặc định Online
        private static string NormalizeStatus(string? s)
            => s == OFFLINE ? OFFLINE : (s == OUT_OF_ORDER ? OUT_OF_ORDER : ONLINE);

        public ChargerService(IChargerRepository repo, IS3Service s3) // UPDATED
        {
            _repo = repo;
            _s3 = s3; // NEW
        }

        // =============== BASIC CRUD ===============

        public async Task<IEnumerable<ChargerReadDto>> GetAllAsync()
        {
            var list = await _repo.GetAllAsync();
            return list.Select(MapToRead);
        }

        public async Task<ChargerReadDto> GetByIdAsync(int id)
        {
            var c = await _repo.GetByIdWithPortsAsync(id);
            if (c == null) throw new KeyNotFoundException("Không tìm thấy charger.");
            return MapToRead(c);
        }

        public async Task<ChargerReadDto> CreateAsync(ChargerCreateDto dto)
        {
            if (await _repo.ExistsCodeAsync(dto.Code))
                throw new InvalidOperationException("Mã charger (Code) đã tồn tại.");


            var entity = new Charger
            {
                StationId = dto.StationId,
                Code = dto.Code,
                Type = dto.Type,
                PowerKw = dto.PowerKw,
                Status = NormalizeStatus(dto.Status), // mặc định Online
                InstalledAt = dto.InstalledAt,
                ImageUrl = dto.ImageUrl,
                CreatedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(entity);

            // trả về kèm Ports (nếu có)
            var withPorts = await _repo.GetByIdWithPortsAsync(entity.ChargerId) ?? entity;
            return MapToRead(withPorts);
        }

        public async Task<bool> UpdateAsync(int id, ChargerUpdateDto dto)
        {
            if (await _repo.ExistsCodeAsync(dto.Code, ignoreId: id))
                throw new InvalidOperationException("Mã charger (Code) đã tồn tại.");

            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return false;

            entity.StationId = dto.StationId;
            entity.Code = dto.Code;
            entity.Type = dto.Type;


            entity.PowerKw = dto.PowerKw;
            if (!string.IsNullOrWhiteSpace(dto.Status) && IsValidStatus(dto.Status.Trim()))
                entity.Status = dto.Status.Trim(); // chỉ nhận 3 trạng thái
            entity.InstalledAt = dto.InstalledAt;
            entity.ImageUrl = dto.ImageUrl;
            entity.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(entity);
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return false;

            await _repo.DeleteAsync(entity);
            return true;
        }

        // =============== PAGING + FILTER ===============

        public async Task<(IEnumerable<ChargerReadDto> Items, int Total)> GetPagedAsync(
            int page, int pageSize,
            int? stationId, string? code, string? type, string? status,
            decimal? minPower, decimal? maxPower)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;

            var total = await _repo.CountAsync(stationId, code, type, status, minPower, maxPower);

            // lấy danh sách kèm Ports để tính Utilization
            var list = await _repo.GetPagedWithPortsAsync(page, pageSize, stationId, status);


            // giữ các filter còn lại tại service cho khớp chữ ký
            if (!string.IsNullOrWhiteSpace(code))
                list = list.Where(c => c.Code != null && c.Code.Contains(code)).ToList();
            if (!string.IsNullOrWhiteSpace(type))
                list = list.Where(c => c.Type == type).ToList();
            if (minPower.HasValue)
                list = list.Where(c => c.PowerKw >= minPower.Value).ToList();
            if (maxPower.HasValue)
                list = list.Where(c => c.PowerKw <= maxPower.Value).ToList();

            return (list.Select(MapToRead), total);
        }

        // =============== CHANGE STATUS ===============

        public Task<bool> ChangeStatusAsync(int id, string status)
        {
            if (!IsValidStatus(status))
                throw new ArgumentException("Status phải là Online / Offline / OutOfOrder.");
            return _repo.UpdateStatusAsync(id, status);
        }


        // =============== MAPPING & UTILIZATION ===============

        private static string? ComputeUtilization(Charger c)
        {
            if (c.Status != ONLINE) return null;

            var ports = c.Ports ?? new List<Port>();
            if (ports.Count == 0) return "Idle";

            int available = ports.Count(p => p.Status == "Available");
            int disabled = ports.Count(p => p.Status == "Disabled");

            if (available == 0) return "Busy";
            if (available == ports.Count - disabled) return "Idle";
            return "Partial";
        }

        private static ChargerReadDto MapToRead(Charger c) => new ChargerReadDto
        {
            ChargerId = c.ChargerId,
            StationId = c.StationId,
            Code = c.Code ?? string.Empty,
            Type = c.Type ?? string.Empty,
            PowerKw = c.PowerKw,
            InstalledAt = c.InstalledAt,
            ImageUrl = c.ImageUrl,
            Status = NormalizeStatus(c.Status),

            // read-only
            Utilization = ComputeUtilization(c),
            TotalPorts = c.Ports?.Count ?? 0,
            AvailablePorts = c.Ports?.Count(p => p.Status == "Available") ?? 0,
            DisabledPorts = c.Ports?.Count(p => p.Status == "Disabled") ?? 0
        };

        // ======================= [IMAGE UPLOAD] =======================
        public async Task<ChargerReadDto> UploadImageAsync(int id, IFormFile file) // NEW
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File rỗng.");

            var ct = (file.ContentType ?? "").ToLower();
            if (!ct.StartsWith("image/"))
                throw new ArgumentException("Chỉ chấp nhận image/*");

            var entity = await _repo.GetByIdAsync(id)
                         ?? throw new KeyNotFoundException("Không tìm thấy charger.");

            // upload lên S3 theo prefix chargers/{id}
            var url = await _s3.UploadFileAsync(file, $"chargers/{id}");

            // (tuỳ chọn) nếu muốn xoá ảnh cũ:
            // if (!string.IsNullOrEmpty(entity.ImageUrl)) await _s3.DeleteFileAsync(entity.ImageUrl);

            entity.ImageUrl = url;
            entity.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(entity);

            return MapToRead(entity); // dùng mapper đang có
        }
    }
}

