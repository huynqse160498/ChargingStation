using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface ISubscriptionRepository
    {
        Task<List<Subscription>> GetAllAsync();                  
        Task<Subscription?> GetByIdAsync(int id);               
        Task<Subscription> AddAsync(Subscription entity);        
        Task<Subscription> UpdateAsync(Subscription entity);     
        Task DeleteAsync(Subscription entity);
        Task<Subscription?> GetActiveByCustomerOrCompanyAsync(int? customerId, int? companyId);

        // ADD: true nếu user đang có Subscription Status Pending/Active
        Task<bool> HasCurrentByCustomerAsync(int customerId);

        // ADD: true nếu company đang có Subscription Status Pending/Active
        Task<bool> HasCurrentByCompanyAsync(int companyId);
    }
}
