using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;
using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories.Implementations
{
    public class SubscriptionRepository : ISubscriptionRepository
    {
        private readonly ChargeStationContext _ctx;
        public SubscriptionRepository(ChargeStationContext ctx) => _ctx = ctx;

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

        // ============================================================
        // 🔹 Ưu tiên gói của Company nếu Customer thuộc công ty
        // ============================================================
        public async Task<Subscription?> GetActiveByCustomerOrCompanyAsync(int? customerId, int? companyId)
        {
            var now = DateTime.Now;

            IQueryable<Subscription> q = _ctx.Subscriptions
                .Include(s => s.SubscriptionPlan)
                .Where(s => (s.Status == "Active" || s.Status == "Pending")
                         && (s.EndDate == null || s.EndDate >= now));

            Subscription? sub = null;

            // Ưu tiên gói cá nhân
            if (customerId.HasValue && customerId > 0)
            {
                sub = await q.Where(s => s.CustomerId == customerId.Value)
                             .OrderByDescending(s => s.StartDate)
                             .FirstOrDefaultAsync();
            }

            // Nếu không có gói cá nhân, thử tìm gói công ty
            if (sub == null && companyId.HasValue && companyId > 0)
            {
                sub = await q.Where(s => s.CompanyId == companyId.Value)
                             .OrderByDescending(s => s.StartDate)
                             .FirstOrDefaultAsync();
            }

            return sub;
        }

        // ============================================================
        // 🔹 Kiểm tra gói còn hiệu lực (cá nhân hoặc doanh nghiệp)
        // ============================================================
        public Task<bool> HasCurrentByCustomerAsync(int customerId)
        {
            var now = DateTime.Now;
            return _ctx.Subscriptions.AnyAsync(s =>
                s.CustomerId == customerId &&
                (s.Status == "Pending" || s.Status == "Active") &&
                (s.EndDate == null || s.EndDate >= now));
        }

        public Task<bool> HasCurrentByCompanyAsync(int companyId)
        {
            var now = DateTime.Now;
            return _ctx.Subscriptions.AnyAsync(s =>
                s.CompanyId == companyId &&
                (s.Status == "Pending" || s.Status == "Active") &&
                (s.EndDate == null || s.EndDate >= now));
        }

        public async Task DeleteAsync(Subscription entity)
        {
            _ctx.Subscriptions.Remove(entity);
            await _ctx.SaveChangesAsync();
        }
    }
}
