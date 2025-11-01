using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface IFeedbackRepository
    {
        Task<Feedback?> GetByIdAsync(int id);
        Task<List<Feedback>> GetByStationAsync(int stationId);
        Task AddAsync(Feedback entity);
        Task UpdateAsync(Feedback entity);
        Task<bool> DeleteAsync(int id);

        // optional: paging/filter
        Task<int> CountAsync(int? stationId, int? customerId, int? rating);
        Task<List<Feedback>> GetPagedAsync(int page, int pageSize, int? stationId, int? customerId, int? rating);
    }
}
