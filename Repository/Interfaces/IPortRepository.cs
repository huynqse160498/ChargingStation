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
        Task<List<Port>> GetAllAsync();
        Task<Port?> GetByIdAsync(int id);
        Task AddAsync(Port port);
        Task UpdateAsync(Port port);
        Task DeleteAsync(Port port);

        // Chặn trùng cổng cùng loại trên cùng một charger (tùy rule )
        Task<bool> ExistsConnectorAsync(int chargerId, string connectorType, int? ignoreId = null);

        // NEW: phân trang + đếm
        Task<int> CountAsync(int? chargerId, string? status);
        Task<List<Port>> GetPagedAsync(int page, int pageSize, int? chargerId, string? status);

        // NEW: đổi trạng thái nhanh
        Task<bool> UpdateStatusAsync(int id, string status);

        Task SaveChangesAsync();
    }
}
