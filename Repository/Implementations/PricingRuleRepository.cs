using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;
using Repositories.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories.Implementations
{
    public class PricingRuleRepository : IPricingRuleRepository
    {
        private readonly ChargeStationContext _context;

        public PricingRuleRepository(ChargeStationContext context)
        {
            _context = context;
        }

        public IQueryable<PricingRule> GetAll()
        {
            return _context.PricingRules.AsQueryable();
        }

        public async Task<PricingRule?> GetByIdAsync(int id)
        {
            return await _context.PricingRules.FirstOrDefaultAsync(x => x.PricingRuleId == id);
        }

        public async Task AddAsync(PricingRule rule)
        {
            await _context.PricingRules.AddAsync(rule);
        }

        public async Task UpdateAsync(PricingRule rule)
        {
            _context.PricingRules.Update(rule);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(PricingRule rule)
        {
            _context.PricingRules.Remove(rule);
            await Task.CompletedTask;
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        // 🔹 Lấy PricingRule đang hoạt động theo loại trụ, công suất và khung giờ
        public async Task<PricingRule?> GetActiveRuleAsync(string chargerType, decimal powerKw, string timeRange)
        {
            return await _context.PricingRules
                .Where(x =>
                    x.Status == "Active" &&
                    x.ChargerType == chargerType &&
                    x.PowerKw == powerKw &&
                    x.TimeRange == timeRange)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();
        }
    }
}
