using System.Threading.Tasks;
using Repositories.DTOs;

namespace Services.Interfaces
{
    public interface IBookingService
    {
        Task<PagedResult<BookingDtos.ListItem>> GetAllAsync(BookingDtos.Query query);
        Task<BookingDtos.Detail?> GetByIdAsync(int id);
        Task<string> CreateAsync(BookingDtos.Create dto);
        Task<string> UpdateAsync(int id, BookingDtos.Update dto);
        Task<string> DeleteAsync(int id);
        Task<string> ChangeStatusAsync(int id, string newStatus);
    }
}
