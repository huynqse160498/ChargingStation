using Repositories.DTOs;
using Repositories.DTOs.Vehicles;
using Repositories.Interfaces;
using Repositories.Models;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services.Implementations
{
    public class VehicleService : IVehicleService
    {
        private readonly IVehicleRepository _repo;

        public VehicleService(IVehicleRepository repo)
        {
            _repo = repo;
        }

        // ===== Queries =====

        public async Task<IEnumerable<VehicleReadDto>> GetAllAsync()
        {
            var data = await _repo.GetAllAsync();
            return data.Select(MapToReadDto);
        }

        public async Task<VehicleReadDto> GetByIdAsync(int id)
        {
            var v = await _repo.GetByIdAsync(id);
            if (v == null) throw new KeyNotFoundException("Không tìm thấy vehicle.");
            return MapToReadDto(v);
        }

        public async Task<PagedResult<VehicleReadDto>> GetPagedAsync(
            int page, int pageSize,
            string? licensePlate, string? carMaker, string? model, string? status,
            int? yearFrom, int? yearTo,
            string? vehicleType // NEW
        )
        {
            var total = await _repo.CountAsync(licensePlate, carMaker, model, status, yearFrom, yearTo, vehicleType); //NEW
            var data = await _repo.GetPagedAsync(page, pageSize, licensePlate, carMaker, model, status, yearFrom, yearTo, vehicleType); //NEW

            // Lưu ý: lớp PagedResult<T> của bạn cần có các thuộc tính: Total, Page, PageSize, Items
            // Nếu tên khác (vd: TotalCount/Data), hãy đổi tên ở đây cho khớp.
            return new PagedResult<VehicleReadDto>
            {
                TotalItems = total,
                Page = page,
                PageSize = pageSize,
                Items = data.Select(MapToReadDto).ToList()
            };
        }

        // ===== Commands =====

        public async Task<VehicleReadDto> CreateAsync(VehicleCreateDto dto)
        {
            var normalizedPlate = dto.LicensePlate?.Trim().ToUpperInvariant();
            if (await _repo.ExistsLicenseAsync(normalizedPlate))
                throw new InvalidOperationException("Biển số đã tồn tại.");

            var v = new Vehicle
            {
                CustomerId = dto.CustomerId,
                CompanyId = dto.CompanyId,
                CarMaker = dto.CarMaker?.Trim(),
                Model = dto.Model?.Trim(),
                LicensePlate = normalizedPlate,
                BatteryCapacity = dto.BatteryCapacity,
                CurrentSoc = dto.CurrentSoc,
                ConnectorType = dto.ConnectorType?.Trim(),
                ManufactureYear = dto.ManufactureYear,
                ImageUrl = dto.ImageUrl?.Trim(),
                VehicleType = dto.VehicleType?.Trim(),
                Status = "Open", //NEW: thống nhất Service dùng Open
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(v);
            return MapToReadDto(v);
        }

        public async Task UpdateAsync(int id, VehicleUpdateDto dto)
        {
            var v = await _repo.GetByIdAsync(id);
            if (v == null) throw new KeyNotFoundException("Không tìm thấy vehicle.");

            var normalizedPlate = dto.LicensePlate?.Trim().ToUpperInvariant();
            if (await _repo.ExistsLicenseAsync(normalizedPlate, ignoreId: id))
                throw new InvalidOperationException("Biển số đã tồn tại.");

            // (Tuỳ chính sách) Không đổi CustomerId ở update → bỏ gán CustomerId.
            v.CompanyId = dto.CompanyId;
            v.CarMaker = dto.CarMaker?.Trim();
            v.Model = dto.Model?.Trim();
            v.LicensePlate = normalizedPlate;
            v.BatteryCapacity = dto.BatteryCapacity;
            v.CurrentSoc = dto.CurrentSoc;
            v.ConnectorType = dto.ConnectorType?.Trim();
            v.ManufactureYear = dto.ManufactureYear;
            v.ImageUrl = dto.ImageUrl?.Trim();
            v.VehicleType = dto.VehicleType?.Trim();
            v.Status = "Open"; //NEW: thống nhất Service dùng Open
            v.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(v);
        }

        public async Task ChangeStatusAsync(int id, string status)
        {
            var v = await _repo.GetByIdAsync(id);
            if (v == null) throw new KeyNotFoundException("Không tìm thấy vehicle.");

            v.Status = (status ?? string.Empty).Trim();
            v.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(v);
        }

        public async Task DeleteAsync(int id)
        {
            var v = await _repo.GetByIdAsync(id);
            if (v == null) throw new KeyNotFoundException("Không tìm thấy vehicle.");

            await _repo.DeleteAsync(v);
        }

        // ===== Mapping =====

        private static VehicleReadDto MapToReadDto(Vehicle v) => new VehicleReadDto
        {
            VehicleId = v.VehicleId,
            CustomerId = v.CustomerId,
            CompanyId = v.CompanyId,
            CarMaker = v.CarMaker,
            Model = v.Model,
            LicensePlate = v.LicensePlate,
            BatteryCapacity = v.BatteryCapacity,
            CurrentSoc = v.CurrentSoc,
            ConnectorType = v.ConnectorType,
            ManufactureYear = v.ManufactureYear,
            CreatedAt = v.CreatedAt,
            UpdatedAt = v.UpdatedAt,
            Status = v.Status,
            ImageUrl = v.ImageUrl,
            VehicleType = v.VehicleType
        };
    }
}

