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
        Task<List<Charger>> GetAllAsync();
        Task<Charger?> GetByIdAsync(int id);
        Task AddAsync(Charger charger);
        Task UpdateAsync(Charger charger);
        Task DeleteAsync(Charger charger);
        Task<bool> ExistsCodeAsync(string code, int? ignoreId = null); // kiểm tra trùng Code

        // NEW
        Task<int> CountAsync(int? stationId, string? code, string? type, string? status,
                             decimal? minPower, decimal? maxPower);
        Task<List<Charger>> GetPagedAsync(int page, int pageSize,
                             int? stationId, string? code, string? type, string? status,
                             decimal? minPower, decimal? maxPower);
        Task<bool> UpdateStatusAsync(int id, string status);

        Task SaveChangesAsync();
    }
}
