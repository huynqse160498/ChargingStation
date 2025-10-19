using Repositories.DTOs.Subscriptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface ISubscriptionService
    {
        Task<IEnumerable<SubscriptionReadDto>> GetAllAsync();                 
        Task<SubscriptionReadDto> GetByIdAsync(int id);                       
        Task<SubscriptionReadDto> CreateAsync(SubscriptionCreateDto dto);     
        Task<SubscriptionReadDto> UpdateAsync(int id, SubscriptionUpdateDto dto);
        Task DeleteAsync(int id);
    }
}
