using Repositories.DTOs;
using Repositories.DTOs.PricingRules;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IPricingRuleService
    {
        Task<PagedResult<PricingRuleListItemDto>> GetAllAsync(PricingRuleQueryDto query);
        Task<PricingRuleDetailDto?> GetByIdAsync(int id);
        Task<string> CreateAsync(PricingRuleCreateDto dto);
        Task<string> UpdateAsync(int id, PricingRuleUpdateDto dto);
        Task<string> ChangeStatusAsync(int id, string newStatus);
        Task<string> DeleteAsync(int id);
    }
}
