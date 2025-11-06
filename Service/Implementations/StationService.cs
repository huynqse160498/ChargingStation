using Repositories.DTOs.Stations;
using Repositories.Interfaces;
using Repositories.Models;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Services.Implementations
{
    public class StationService : IStationService
    {
        private readonly IStationRepository _repo;
        private readonly IS3Service _s3;

        public StationService(IStationRepository repo, IS3Service s3)
        {
            _repo = repo;
            _s3 = s3;
        }

        // NEW: whitelist 2 trạng thái
        private const string OPEN = "Open";
        private const string CLOSED = "Closed";
        private static string Normalize(string? s) => s == CLOSED ? CLOSED : OPEN; // NEW


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
                Status = Normalize(dto.Status),
                //ImageUrl = dto.ImageUrl,
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
            entity.Status = string.IsNullOrWhiteSpace(dto.Status) ? entity.Status : Normalize(dto.Status);
            //entity.ImageUrl = dto.ImageUrl;
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
            => _repo.UpdateStatusAsync(id, Normalize(status));

        // Map entity -> DTO
        private static StationReadDto MapToRead(Station s) => new StationReadDto
        {
            StationId = s.StationId,
            StationName = s.StationName,
            Address = s.Address,
            City = s.City,
            Latitude = s.Latitude,
            Longitude = s.Longitude,
            Status = Normalize(s.Status),
            ImageUrl = s.ImageUrl
        };

        // ======================= [IMAGE UPLOAD] =======================
        public async Task<StationReadDto> UploadImageAsync(int id, IFormFile file) // NEW
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File rỗng.");

            var contentType = (file.ContentType ?? "").ToLower();
            if (!contentType.StartsWith("image/"))
                throw new ArgumentException("Chỉ chấp nhận image/*");

            // lấy entity (tracking) để cập nhật
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) throw new KeyNotFoundException("Không tìm thấy station.");

            // upload: folder theo chuẩn stations/{id}
            var url = await _s3.UploadFileAsync(file, $"stations/{id}");

            // (tuỳ) nếu muốn xoá ảnh cũ:
            // if (!string.IsNullOrEmpty(entity.ImageUrl)) await _s3.DeleteFileAsync(entity.ImageUrl);

            entity.ImageUrl = url;
            entity.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(entity);

            return MapToRead(entity);
        }
    }
}
