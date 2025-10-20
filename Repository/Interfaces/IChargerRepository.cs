using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface IChargerRepository
    {
        //CRUD CƠ BẢN
        Task<List<Charger>> GetAllAsync();
        Task<Charger?> GetByIdAsync(int id);
        Task AddAsync(Charger charger);
        Task UpdateAsync(Charger charger);
        Task DeleteAsync(Charger charger);


        // Kiểm tra trùng mã Charger (Code)
        Task<bool> ExistsCodeAsync(string code, int? ignoreId = null);

        // ======================= [FILTER + PAGING] =======================
        Task<int> CountAsync(int? stationId, string? code, string? type, string? status,
                             decimal? minPower, decimal? maxPower);

        Task<List<Charger>> GetPagedAsync(int page, int pageSize,
                             int? stationId, string? code, string? type, string? status,
                             decimal? minPower, decimal? maxPower);


        // ======================= [READ WITH PORTS] =======================
        // Dùng để đọc thêm danh sách Port nhằm tính Utilization (Idle, Partial, Busy)
        Task<Charger?> GetByIdWithPortsAsync(int id);
        Task<List<Charger>> GetPagedWithPortsAsync(
            int page, int pageSize, int? stationId = null, string? status = null);

        // ======================= [UPDATE STATUS] =======================
        // Cập nhật nhanh trạng thái Online / Offline / OutOfOrder
        Task<bool> UpdateStatusAsync(int id, string status);

        // Lưu thay đổi
        Task SaveChangesAsync();
    }
}
