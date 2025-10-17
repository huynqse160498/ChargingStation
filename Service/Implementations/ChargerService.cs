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

        // ===================== NEW: 3 trạng thái vận hành =====================
        private const string ONLINE = "Online";        // mặc định
        private const string OFFLINE = "Offline";
        private const string OUT_OF_ORDER = "OutOfOrder";

        private static bool IsValidStatus(string? s)   // NEW
            => s == ONLINE || s == OFFLINE || s == OUT_OF_ORDER;

        public ChargerService(IChargerRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<ChargerReadDto>> GetAllAsync()
        {
            // Giữ nguyên: đọc quick không kèm Ports -> không có Utilization
            var list = await _repo.GetAllAsync();
            return list.Select(MapToRead);
        }

        public async Task<ChargerReadDto> GetByIdAsync(int id)
        {
            // NEW: đọc kèm Ports để tính Utilization
            var c = await _repo.GetByIdWithPortsAsync(id); // NEW
            if (c == null) throw new KeyNotFoundException("Không tìm thấy charger.");
            return MapToRead(c);                            // NEW
        }

        public async Task<ChargerReadDto> CreateAsync(ChargerCreateDto dto)
        {
            if (await _repo.ExistsCodeAsync(dto.Code))
                throw new InvalidOperationException("Mã charger (Code) đã tồn tại.");

            // NEW: chuẩn hoá status 3 trạng thái, mặc định Online
            var status = string.IsNullOrWhiteSpace(dto.Status) ? ONLINE : dto.Status!.Trim();
            if (!IsValidStatus(status)) status = ONLINE;

            var entity = new Charger
            {
                StationId = dto.StationId,
                Code = dto.Code,
                Type = dto.Type,
                PowerKw = dto.PowerKw, // decimal?
                Status = status,      // NEW: Online/Offline/OutOfOrder
                InstalledAt = dto.InstalledAt,
                ImageUrl = dto.ImageUrl,
                CreatedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(entity);

            // NEW: đọc lại kèm Ports để trả ra Utilization
            var withPorts = await _repo.GetByIdWithPortsAsync(entity.ChargerId) ?? entity;
            return MapToRead(withPorts); // NEW
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
            entity.PowerKw = dto.PowerKw;   // decimal?

            // ===================== NEW: chỉ set 3 trạng thái hợp lệ =====================
            if (!string.IsNullOrWhiteSpace(dto.Status))
            {
                var s = dto.Status.Trim();
                if (IsValidStatus(s))
                    entity.Status = s;
            }
            else
            {
                // nếu update không gửi status thì giữ nguyên; KHÔNG ép "Open" nữa
            }
            // ===========================================================================

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

        // NEW: paging + filter (trả về kèm Utilization)
        public async Task<(IEnumerable<ChargerReadDto> Items, int Total)> GetPagedAsync(
            int page, int pageSize,
            int? stationId, string? code, string? type, string? status,
            decimal? minPower, decimal? maxPower)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;

            var total = await _repo.CountAsync(stationId, code, type, status, minPower, maxPower);

            // NEW: lấy danh sách kèm Ports để tính Utilization
            var list = await _repo.GetPagedWithPortsAsync(page, pageSize, stationId, status); // NEW

            // Giữ các filter code/type/power ở service nếu cần khớp 100%
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

        // NEW: change-status với whitelist 3 trạng thái
        public Task<bool> ChangeStatusAsync(int id, string status)
        {
            if (!IsValidStatus(status))
                throw new ArgumentException("Status phải là Online | Offline | OutOfOrder.");
            return _repo.UpdateStatusAsync(id, status);
        }

        // ===================== NEW: tính Utilization theo Ports =====================
        // Idle    : tất cả "usable" ports đều Available
        // Busy    : không còn Port Available
        // Partial : còn Available nhưng cũng có Reserved/Occupied/Disabled
        // Status != Online => Utilization = null
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
        // ==========================================================================

        // NEW: MapToRead có thêm Utilization + counters
        private static ChargerReadDto MapToRead(Charger c) => new ChargerReadDto
        {
            ChargerId = c.ChargerId,
            StationId = c.StationId,
            Code = c.Code,
            Type = c.Type,
            PowerKw = c.PowerKw,
            Status = IsValidStatus(c.Status) ? c.Status : ONLINE, // normalize
            InstalledAt = c.InstalledAt,
            ImageUrl = c.ImageUrl,

            // ===== NEW: chỉ hiện khi Online =====
            Utilization = ComputeUtilization(c),
            TotalPorts = c.Ports?.Count ?? 0,
            AvailablePorts = c.Ports?.Count(p => p.Status == "Available") ?? 0,
            DisabledPorts = c.Ports?.Count(p => p.Status == "Disabled") ?? 0
        };
    }
}
