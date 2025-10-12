using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface IStationRepository
    {
        Task<List<Station>> GetAllAsync();
        Task<Station?> GetByIdAsync(int id);
        Task AddAsync(Station station);
        Task UpdateAsync(Station station);
        Task DeleteAsync(Station station);

        // Chặn trùng tên trạm (cần có thể thay bằng rule khác )
        Task<bool> ExistsNameAsync(string stationName, int? ignoreId = null);

        // NEW: đếm + trang + filter
        Task<int> CountAsync(string? stationName, string? city, string? status);
        Task<List<Station>> GetPagedAsync(int page, int pageSize, string? stationName, string? city, string? status);

        // NEW: đổi trạng thái nhanh
        Task<bool> UpdateStatusAsync(int id, string status);

        Task SaveChangesAsync();
    }
}
