using Repositories.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface IPricingRuleRepository
    {
        IQueryable<PricingRule> GetAll();
        Task<PricingRule?> GetByIdAsync(int id);
        Task AddAsync(PricingRule rule);
        Task UpdateAsync(PricingRule rule);
        Task DeleteAsync(PricingRule rule);
        Task SaveAsync();

        // 🔹 Thêm hàm lấy quy tắc giá theo loại trụ, công suất và khung giờ
        Task<PricingRule?> GetActiveRuleAsync(string chargerType, decimal powerKw, string timeRange);
    }
}
