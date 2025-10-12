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

        public async Task<VehicleReadDto> CreateAsync(VehicleCreateDto dto)
        {
            if (await _repo.ExistsLicenseAsync(dto.LicensePlate))
                throw new InvalidOperationException("Biển số đã tồn tại.");

            var entity = new Vehicle
            {
                CarMaker = dto.CarMaker,
                Model = dto.Model,
                LicensePlate = dto.LicensePlate,
                ManufactureYear = dto.ManufactureYear,
                BatteryCapacity = dto.BatteryCapacity, // decimal?
                ConnectorType = dto.ConnectorType,
                ImageUrl = dto.ImageUrl,
                Status = "Open",
                CreatedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(entity);
            return MapToReadDto(entity);
        }

        public async Task<bool> UpdateAsync(int id, VehicleUpdateDto dto)
        {
            if (await _repo.ExistsLicenseAsync(dto.LicensePlate, ignoreId: id))
                throw new InvalidOperationException("Biển số đã tồn tại.");

            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return false;

            entity.CarMaker = dto.CarMaker;
            entity.Model = dto.Model;
            entity.LicensePlate = dto.LicensePlate;
            entity.ManufactureYear = dto.ManufactureYear;
            entity.BatteryCapacity = dto.BatteryCapacity; // decimal?
            entity.ConnectorType = dto.ConnectorType;
            entity.ImageUrl = dto.ImageUrl;
            entity.Status = "Open";
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
        public async Task<(IEnumerable<VehicleReadDto> Items, int Total)> GetPagedAsync(
            int page, int pageSize,
            string? licensePlate, string? carMaker, string? model, string? status,
            int? yearFrom, int? yearTo)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;

            var total = await _repo.CountAsync(licensePlate, carMaker, model, status, yearFrom, yearTo);
            var list = await _repo.GetPagedAsync(page, pageSize, licensePlate, carMaker, model, status, yearFrom, yearTo);
            return (list.Select(MapToReadDto), total);
        }

        // NEW: đổi status
        public async Task<bool> ChangeStatusAsync(int id, string status)
            => await _repo.UpdateStatusAsync(id, status);

        // Map entity -> dto (đồng bộ decimal?)
        private static VehicleReadDto MapToReadDto(Vehicle v) => new VehicleReadDto
        {
            VehicleId = v.VehicleId,
            CarMaker = v.CarMaker,
            Model = v.Model,
            LicensePlate = v.LicensePlate,
            ManufactureYear = v.ManufactureYear,
            BatteryCapacity = v.BatteryCapacity,
            ConnectorType = v.ConnectorType,
            ImageUrl = v.ImageUrl,
            Status = v.Status
        };

    }
}
