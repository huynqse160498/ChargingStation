using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface IVehicleRepository
    {
        Task<List<Vehicle>> GetAllAsync();
        Task<Vehicle> GetByIdAsync(int id);
        Task AddAsync(Vehicle vehicle);
        Task UpdateAsync(Vehicle vehicle);
        Task DeleteAsync(Vehicle vehicle);
        Task<bool> ExistsLicenseAsync(string licensePlate, int? ignoreId = null);

        // NEW: đếm + phân trang + filter
        Task<int> CountAsync(string? licensePlate, string? carMaker, string? model, string? status,
                             int? yearFrom, int? yearTo);
        Task<List<Vehicle>> GetPagedAsync(int page, int pageSize,
                                          string? licensePlate, string? carMaker, string? model, string? status,
                                          int? yearFrom, int? yearTo);

        // NEW: đổi trạng thái nhanh
        Task<bool> UpdateStatusAsync(int id, string status);


        Task SaveChangesAsync();
    }
}
