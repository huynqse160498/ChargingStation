using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface ISubscriptionPlanRepository
    {
        Task<List<SubscriptionPlan>> GetAllAsync();                                   
        Task<SubscriptionPlan?> GetByIdAsync(int id);                                 
        Task<bool> ExistsNameAsync(string planName, int? ignoreId = null);            
        Task<SubscriptionPlan> AddAsync(SubscriptionPlan entity);                     
        Task<SubscriptionPlan> UpdateAsync(SubscriptionPlan entity);                 
        Task DeleteAsync(SubscriptionPlan entity);
    }
}
