using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;
using Repositories.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories.Implementations
{
    public class PricingRuleRepository : IPricingRuleRepository
    {
        private readonly ChargeStationContext _db;

        public PricingRuleRepository(ChargeStationContext db)
        {
            _db = db;
        }

        public IQueryable<PricingRule> GetAll()
            => _db.PricingRules.AsNoTracking().AsQueryable();

        public async Task<PricingRule?> GetByIdAsync(int id)
            => await _db.PricingRules.FirstOrDefaultAsync(x => x.PricingRuleId == id);

        public async Task<PricingRule?> GetActiveRuleAsync(string vehicleType, string timeRange)
            => await _db.PricingRules
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.VehicleType == vehicleType
                                               && x.TimeRange == timeRange
                                               && x.Status == "Active");

        public async Task AddAsync(PricingRule entity)
            => await _db.PricingRules.AddAsync(entity);

        public async Task UpdateAsync(PricingRule entity)
        {
            _db.PricingRules.Update(entity);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(PricingRule entity)
        {
            _db.PricingRules.Remove(entity);
            await Task.CompletedTask;
        }

        public async Task SaveAsync()
            => await _db.SaveChangesAsync();
    }
}
