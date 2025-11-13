using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repositories.DTOs.Ports;
using Repositories.Interfaces;
using Repositories.Models;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implementations
{
    public class PortService : IPortService
    {
        private readonly IPortRepository _repo;
        private readonly IS3Service _s3;

        public PortService(IPortRepository repo, IS3Service s3)
        {
            _repo = repo;
            _s3 = s3;
        }

        public async Task<IEnumerable<PortReadDto>> GetAllAsync()
        {
            var list = await _repo.GetAllAsync();
            return list.Select(MapToRead);
        }

        public async Task<PortReadDto> GetByIdAsync(int id)
        {
            var p = await _repo.GetByIdAsync(id);
            if (p == null) throw new KeyNotFoundException("Không tìm thấy port.");
            return MapToRead(p);
        }

        public async Task<PortReadDto> CreateAsync(PortCreateDto dto)
        {
            if (await _repo.ExistsConnectorAsync(dto.ChargerId, dto.ConnectorType))
                throw new InvalidOperationException("ConnectorType đã tồn tại trên charger này.");

            // NEW: default status = Available
            var entity = new Port
            {
                ChargerId = dto.ChargerId,
                ConnectorType = dto.ConnectorType,
                MaxPowerKw = dto.MaxPowerKw,   // decimal?
                Status = "Available",
                //ImageUrl = dto.ImageUrl,
                CreatedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(entity);
            return MapToRead(entity);
        }

        public async Task<bool> UpdateAsync(int id, PortUpdateDto dto)
        {
            if (await _repo.ExistsConnectorAsync(dto.ChargerId, dto.ConnectorType, ignoreId: id))
                throw new InvalidOperationException("ConnectorType đã tồn tại trên charger này.");

            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return false;

            entity.ChargerId = dto.ChargerId;
            entity.ConnectorType = dto.ConnectorType;
            entity.MaxPowerKw = dto.MaxPowerKw;
            entity.Status = string.IsNullOrWhiteSpace(dto.Status) ? "Available" : dto.Status;
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

        // NEW: phân trang
        public async Task<(IEnumerable<PortReadDto> Items, int Total)> GetPagedAsync(
            int page, int pageSize, int? chargerId, string? status)
        {
            var total = await _repo.CountAsync(chargerId, status);
            var list = await _repo.GetPagedAsync(page, pageSize, chargerId, status);
            return (list.Select(MapToRead), total);
        }


        // NEW: đổi status 4 trạng thái 
        public async Task<bool> ChangeStatusAsync(int id, string status)

        {
            string[] allowed = { "Available", "Reserved", "Occupied", "Disabled" };
            if (!allowed.Contains(status))
                throw new ArgumentException("Status không hợp lệ. Dùng: Available / Reserved / Occupied / Disabled.");

            return await _repo.UpdateStatusAsync(id, status);
        }

        private static PortReadDto MapToRead(Port p) => new PortReadDto
        {
            PortId = p.PortId,
            ChargerId = p.ChargerId,
            ConnectorType = p.ConnectorType,
            MaxPowerKw = p.MaxPowerKw,
            Status = p.Status,
            ImageUrl = p.ImageUrl
        };
        // ======================= [IMAGE UPLOAD] =======================
        public async Task<PortReadDto> UploadImageAsync(int id, IFormFile file) // NEW
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File rỗng.");
            var ct = (file.ContentType ?? "").ToLower();
            if (!ct.StartsWith("image/"))
                throw new ArgumentException("Chỉ chấp nhận image/*");

            var entity = await _repo.GetByIdAsync(id)
                         ?? throw new KeyNotFoundException("Không tìm thấy port.");

            // Prefix riêng cho port
            var url = await _s3.UploadFileAsync(file, $"ports/{id}");

            // (tuỳ chỉnh) nếu muốn xoá ảnh cũ:
            // if (!string.IsNullOrEmpty(entity.ImageUrl)) await _s3.DeleteFileAsync(entity.ImageUrl);

            entity.ImageUrl = url;            // đảm bảo Port có cột ImageUrl
            entity.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(entity);

            return MapToRead(entity);         // dùng mapper sẵn có trong service
        }
    }
}

