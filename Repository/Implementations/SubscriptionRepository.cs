using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;
using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Implementations
{
    public class SubscriptionRepository : ISubscriptionRepository
    {
        private readonly ChargeStationContext _ctx;
        public SubscriptionRepository(ChargeStationContext ctx) => _ctx = ctx; // NEW

        // ======================= [ BASIC CRUD ] =======================
        public async Task<List<Subscription>> GetAllAsync()
            => await _ctx.Subscriptions
                .Include(x => x.SubscriptionPlan)
                .AsNoTracking()
                .ToListAsync();

        public Task<Subscription?> GetByIdAsync(int id)
            => _ctx.Subscriptions
                .Include(x => x.SubscriptionPlan)
                .FirstOrDefaultAsync(x => x.SubscriptionId == id);

        public async Task<Subscription> AddAsync(Subscription entity)
        {
            _ctx.Subscriptions.Add(entity);
            await _ctx.SaveChangesAsync();
            return entity;
        }

        public async Task<Subscription> UpdateAsync(Subscription entity)
        {
            _ctx.Subscriptions.Update(entity);
            await _ctx.SaveChangesAsync();
            return entity;
        }
        public async Task<Subscription?> GetActiveByCustomerOrCompanyAsync(int customerId, int? companyId)
        {
            return await _ctx.Subscriptions
                .Include(s => s.SubscriptionPlan)
                .Where(s =>
                    (s.CustomerId == customerId || (companyId != null && s.CompanyId == companyId))
                    && s.Status == "Active"
                    && (s.EndDate == null || s.EndDate >= DateTime.Now))
                .OrderByDescending(s => s.StartDate)
                .FirstOrDefaultAsync();
        }


        public async Task DeleteAsync(Subscription entity)
        {
            _ctx.Subscriptions.Remove(entity);
            await _ctx.SaveChangesAsync();
        }
    }
}

