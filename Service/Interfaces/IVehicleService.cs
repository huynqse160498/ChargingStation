using Repositories.DTOs;
using Repositories.DTOs.Vehicles;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IVehicleService
    {
        Task<IEnumerable<VehicleReadDto>> GetAllAsync();
        Task<VehicleReadDto> GetByIdAsync(int id);

        Task<PagedResult<VehicleReadDto>> GetPagedAsync(
            int page,
            int pageSize,
            string licensePlate,
            string carMaker,
            string model,
            string status,
            int? yearFrom,
            int? yearTo,
            string vehicleType 
        );

        Task<VehicleReadDto> CreateAsync(VehicleCreateDto dto);
        Task UpdateAsync(int id, VehicleUpdateDto dto);
        Task ChangeStatusAsync(int id, string status);
        Task DeleteAsync(int id);
    }
}
