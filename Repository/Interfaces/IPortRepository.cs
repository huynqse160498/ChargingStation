using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface IPortRepository
    {
        // Lấy toàn bộ danh sách Port (ít dùng, chủ yếu cho test/admin)
        Task<List<Port>> GetAllAsync();

        // Lấy 1 Port theo Id
        Task<Port?> GetByIdAsync(int id);

        // CRUD cơ bản
        Task AddAsync(Port port);
        Task UpdateAsync(Port port);
        Task DeleteAsync(Port port);


        // Kiểm tra trùng ConnectorType trong cùng 1 Charger
        // (để tránh 1 charger có 2 cổng cùng loại, tuỳ rule)
        Task<bool> ExistsConnectorAsync(int chargerId, string connectorType, int? ignoreId = null);


        // NEW: phân trang + đếm (lọc theo chargerId, status)
        Task<int> CountAsync(int? chargerId, string? status);
        Task<List<Port>> GetPagedAsync(int page, int pageSize, int? chargerId, string? status);


        // NEW: đổi trạng thái nhanh (Available / Reserved / Occupied / Disabled)
        Task<bool> UpdateStatusAsync(int id, string status);

        IQueryable<Port> Query();

        // Lưu thay đổi
        Task SaveChangesAsync();
    }
}
