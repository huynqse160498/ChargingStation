using Repositories.DTOs.Stations;
using Repositories.Interfaces;
using Repositories.Models;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services.Implementations
{
    public class StationService : IStationService
    {
        private readonly IStationRepository _repo;

        public StationService(IStationRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<StationReadDto>> GetAllAsync()
        {
            var list = await _repo.GetAllAsync();
            return list.Select(MapToRead);
        }

        public async Task<StationReadDto> GetByIdAsync(int id)
        {
            var s = await _repo.GetByIdAsync(id);
            if (s == null) throw new KeyNotFoundException("Không tìm thấy station.");
            return MapToRead(s);
        }

        public async Task<StationReadDto> CreateAsync(StationCreateDto dto)
        {
            if (await _repo.ExistsNameAsync(dto.StationName))
                throw new InvalidOperationException("StationName đã tồn tại.");

            var entity = new Station
            {
                StationName = dto.StationName,
                Address = dto.Address,
                City = dto.City,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                Status = dto.Status,
                ImageUrl = dto.ImageUrl,
                CreatedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(entity);
            return MapToRead(entity);
        }

        public async Task<bool> UpdateAsync(int id, StationUpdateDto dto)
        {
            if (await _repo.ExistsNameAsync(dto.StationName, ignoreId: id))
                throw new InvalidOperationException("StationName đã tồn tại.");

            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return false;

            entity.StationName = dto.StationName;
            entity.Address = dto.Address;
            entity.City = dto.City;
            entity.Latitude = dto.Latitude;
            entity.Longitude = dto.Longitude;
            entity.Status = dto.Status;
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

        // NEW: phân trang + filter
        public async Task<(IEnumerable<StationReadDto> Items, int Total)> GetPagedAsync(
            int page, int pageSize, string? stationName, string? city, string? status)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;

            var total = await _repo.CountAsync(stationName, city, status);
            var list = await _repo.GetPagedAsync(page, pageSize, stationName, city, status);
            return (list.Select(MapToRead), total);
        }

        // NEW: đổi status
        public Task<bool> ChangeStatusAsync(int id, string status)
            => _repo.UpdateStatusAsync(id, status);

        // Map entity -> DTO
        private static StationReadDto MapToRead(Station s) => new StationReadDto
        {
            StationId = s.StationId,
            StationName = s.StationName,
            Address = s.Address,
            City = s.City,
            Latitude = s.Latitude,
            Longitude = s.Longitude,
            Status = s.Status,
            ImageUrl = s.ImageUrl
        };
    }
}
