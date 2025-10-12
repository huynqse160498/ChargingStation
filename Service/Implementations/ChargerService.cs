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

        public ChargerService(IChargerRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<ChargerReadDto>> GetAllAsync()
        {
            var list = await _repo.GetAllAsync();
            return list.Select(MapToRead);
        }

        public async Task<ChargerReadDto> GetByIdAsync(int id)
        {
            var c = await _repo.GetByIdAsync(id);
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
                PowerKw = dto.PowerKw, // decimal?
                Status = dto.Status,
                InstalledAt = dto.InstalledAt,
                ImageUrl = dto.ImageUrl,
                CreatedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(entity);
            return MapToRead(entity);
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
            entity.Status = dto.Status;
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

        // NEW: paging + filter
        public async Task<(IEnumerable<ChargerReadDto> Items, int Total)> GetPagedAsync(
            int page, int pageSize,
            int? stationId, string? code, string? type, string? status,
            decimal? minPower, decimal? maxPower)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;

            var total = await _repo.CountAsync(stationId, code, type, status, minPower, maxPower);
            var list = await _repo.GetPagedAsync(page, pageSize, stationId, code, type, status, minPower, maxPower);
            return (list.Select(MapToRead), total);
        }

        // NEW: change-status
        public Task<bool> ChangeStatusAsync(int id, string status)
            => _repo.UpdateStatusAsync(id, status);

        private static ChargerReadDto MapToRead(Charger c) => new ChargerReadDto
        {
            ChargerId = c.ChargerId,
            StationId = c.StationId,
            Code = c.Code,
            Type = c.Type,
            PowerKw = c.PowerKw,
            Status = c.Status,
            InstalledAt = c.InstalledAt,
            ImageUrl = c.ImageUrl
        };
    }
}
