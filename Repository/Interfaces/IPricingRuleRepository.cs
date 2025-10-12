using Repositories.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface IPricingRuleRepository
    {
        IQueryable<PricingRule> GetAll();
        Task<PricingRule?> GetByIdAsync(int id);
        Task<PricingRule?> GetActiveRuleAsync(string vehicleType, string timeRange);
        Task AddAsync(PricingRule entity);
        Task UpdateAsync(PricingRule entity);
        Task DeleteAsync(PricingRule entity);
        Task SaveAsync();
    }
}
