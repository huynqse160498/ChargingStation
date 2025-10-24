using Repositories.DTOs.Notifications;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface INotificationService
    {
        Task<IEnumerable<NotificationReadDto>> GetAllAsync();
        Task<IEnumerable<NotificationReadDto>> GetByCustomerAsync(int customerId, bool includeArchived = false);
        Task<IEnumerable<NotificationReadDto>> GetByCompanyAsync(int companyId, bool includeArchived = false);
        Task<NotificationReadDto> GetByIdAsync(int id);

        Task<NotificationReadDto> CreateAsync(NotificationCreateDto dto);
        Task MarkAsReadAsync(int id);
        Task ArchiveAsync(int id);
        Task DeleteAsync(int id);

        // 🔹 Admin-special
        Task<NotificationReadDto> AdminSendToCustomerAsync(AdminSendToCustomerDto dto);
        Task<NotificationReadDto> AdminSendToCompanyAsync(AdminSendToCompanyDto dto);
        Task<int> AdminBroadcastAsync(AdminBroadcastDto dto); // returns count
    }
}
