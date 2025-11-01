using Repositories.DTOs.Feedbacks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IFeedbackService
    {
        Task<FeedbackReadDto> GetAsync(int id);
        Task<List<FeedbackReadDto>> ListByStationAsync(int stationId);
        Task<(int total, List<FeedbackReadDto> items)> GetPagedAsync(int page, int pageSize, int? stationId, int? customerId, int? rating);
        Task<FeedbackReadDto> CreateAsync(FeedbackCreateDto dto);
        Task<FeedbackReadDto> UpdateAsync(int id, FeedbackUpdateDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
