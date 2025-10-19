using Repositories.DTOs.SubscriptionPlans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Services.Interfaces
{
    public interface ISubscriptionPlanService
    {
        Task<IEnumerable<SubscriptionPlanReadDto>> GetAllAsync();                 
        Task<SubscriptionPlanReadDto> GetByIdAsync(int id);                       
        Task<SubscriptionPlanReadDto> CreateAsync(SubscriptionPlanCreateDto dto);
        Task<SubscriptionPlanReadDto> UpdateAsync(int id, SubscriptionPlanUpdateDto dto);
        Task DeleteAsync(int id);                                                 
    }
}
