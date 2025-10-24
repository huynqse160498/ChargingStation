using Repositories.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface INotificationRepository
    {
        Task<List<Notification>> GetAllAsync();
        Task<List<Notification>> GetByCustomerAsync(int customerId);
        Task<List<Notification>> GetByCompanyAsync(int companyId);
        Task<Notification?> GetByIdAsync(int id);
        Task<Notification> AddAsync(Notification entity);
        Task<Notification> UpdateAsync(Notification entity);
        Task DeleteAsync(Notification entity);
    }
}
