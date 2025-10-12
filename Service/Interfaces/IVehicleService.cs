using Repositories.DTOs.Vehicles;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    
    public interface IVehicleService
    {
        Task<IEnumerable<VehicleReadDto>> GetAllAsync();
        Task<VehicleReadDto> GetByIdAsync(int id);
        Task<VehicleReadDto> CreateAsync(VehicleCreateDto dto);
        Task<bool> UpdateAsync(int id, VehicleUpdateDto dto);
        Task<bool> DeleteAsync(int id);

        // NEW
        Task<(IEnumerable<VehicleReadDto> Items, int Total)> GetPagedAsync(
            int page, int pageSize,
            string? licensePlate, string? carMaker, string? model, string? status,
            int? yearFrom, int? yearTo);

        Task<bool> ChangeStatusAsync(int id, string status);
    }
}