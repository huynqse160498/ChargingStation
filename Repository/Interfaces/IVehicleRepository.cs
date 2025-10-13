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
        // Lấy toàn bộ (ít dùng trong production, chủ yếu admin/test)
        Task<List<Vehicle>> GetAllAsync();

        // Lấy theo ID
        Task<Vehicle> GetByIdAsync(int id);

        // CRUD cơ bản
        Task AddAsync(Vehicle vehicle);
        Task UpdateAsync(Vehicle vehicle);
        Task DeleteAsync(Vehicle vehicle);

        // Kiểm tra trùng biển số (bỏ qua 1 id khi update)
        Task<bool> ExistsLicenseAsync(string licensePlate, int? ignoreId = null);

        // Đếm số lượng xe với filter (phân trang)
        Task<int> CountAsync(
            string? licensePlate,
            string? carMaker,
            string? model,
            string? status,
            int? yearFrom,
            int? yearTo,
            string? vehicleType //NEW: cho phép null để đồng bộ controller/service
        );

        // Lấy danh sách xe phân trang + filter cơ bản
        Task<List<Vehicle>> GetPagedAsync(
            int page,
            int pageSize,
            string? licensePlate,
            string? carMaker,
            string? model,
            string? status,
            int? yearFrom,
            int? yearTo,
            string? vehicleType //NEW: cho phép null để đồng bộ controller/service
        );

        //NEW: đổi trạng thái nhanh
        Task<bool> UpdateStatusAsync(int id, string status);

        //NEW: gọi khi dùng pattern UnitOfWork (nếu cần)
        Task SaveChangesAsync();
    }
}
