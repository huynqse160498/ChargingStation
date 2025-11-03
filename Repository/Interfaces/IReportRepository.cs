using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface IReportRepository
    {
        Task<Report?> GetByIdAsync(int id);

        // List nhanh theo station
        Task<List<Report>> GetByStationAsync(int stationId);

        // CRUD
        Task AddAsync(Report entity);
        Task UpdateAsync(Report entity);
        Task<bool> DeleteAsync(int id);

        // Paging + filter
        Task<int> CountAsync(
            int? stationId, int? chargerId, string? status, string? severity,
            DateTime? from, DateTime? to);

        Task<List<Report>> GetPagedAsync(
            int page, int pageSize,
            int? stationId, int? chargerId, string? status, string? severity,
            DateTime? from, DateTime? to);
    }
}

