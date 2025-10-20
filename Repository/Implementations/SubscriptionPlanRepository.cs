using Microsoft.EntityFrameworkCore;
using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repositories.Interfaces;

namespace Repositories.Implementations
{
    public class SubscriptionPlanRepository : ISubscriptionPlanRepository
    {
        private readonly ChargeStationContext _ctx;
        public SubscriptionPlanRepository(ChargeStationContext ctx) { _ctx = ctx; }

        // ======================= [ BASIC CRUD ] ===============================
        public Task<List<SubscriptionPlan>> GetAllAsync()
            => _ctx.SubscriptionPlans.AsNoTracking().ToListAsync();                 

        public Task<SubscriptionPlan?> GetByIdAsync(int id)
            => _ctx.SubscriptionPlans.FirstOrDefaultAsync(p => p.SubscriptionPlanId == id); 


        public async Task<SubscriptionPlan> AddAsync(SubscriptionPlan entity)
        {
            _ctx.SubscriptionPlans.Add(entity);                                     
            await _ctx.SaveChangesAsync();                                           
            return entity;                                                           
        }

        public async Task<SubscriptionPlan> UpdateAsync(SubscriptionPlan entity)
        {
            _ctx.SubscriptionPlans.Update(entity);                                  
            await _ctx.SaveChangesAsync();                                           
            return entity;                                                          
        }

        public async Task DeleteAsync(SubscriptionPlan entity)
        {
            _ctx.SubscriptionPlans.Remove(entity);                                  
            await _ctx.SaveChangesAsync();                                          
        }
        // ======================= [ VALIDATION / CHECKS ] ======================
        public Task<bool> ExistsNameAsync(string planName, int? ignoreId = null)
            => _ctx.SubscriptionPlans.AnyAsync(p =>
                   p.PlanName == planName && (ignoreId == null || p.SubscriptionPlanId != ignoreId));
    }
}
