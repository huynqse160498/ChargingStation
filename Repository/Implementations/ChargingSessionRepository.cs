using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;
using Repositories.Models;

namespace Repositories.Implementations
{
    public class ChargingSessionRepository : IChargingSessionRepository
    {
        private readonly ChargeStationContext _context;

        public ChargingSessionRepository(ChargeStationContext context)
        {
            _context = context;
        }

        // ============================================================
        // 🔹 Lấy toàn bộ phiên sạc (kèm Invoice + Subscription + Plan)
        // ============================================================
        public async Task<List<ChargingSession>> GetAllAsync()
        {
            return await _context.ChargingSessions
                .Include(x => x.PricingRule)
                .Include(x => x.Customer)
                .Include(x => x.Company)
                .Include(x => x.Vehicle)
                .Include(x => x.Port)
                    .ThenInclude(p => p.Charger)
                .Include(x => x.Invoice) // ✅ Thêm Include Invoice
                    .ThenInclude(i => i.Subscription)
                        .ThenInclude(s => s.SubscriptionPlan)
                .AsNoTracking()
                .ToListAsync();
        }

        // ============================================================
        // 🔹 Lấy chi tiết 1 phiên sạc
        // ============================================================
        public async Task<ChargingSession?> GetByIdAsync(int id)
        {
            return await _context.ChargingSessions
                .Include(x => x.PricingRule)
                .Include(x => x.Customer)
                .Include(x => x.Company)
                .Include(x => x.Vehicle)
                .Include(x => x.Port)
                    .ThenInclude(p => p.Charger)
                .Include(x => x.Invoice)
                    .ThenInclude(i => i.Subscription)
                        .ThenInclude(s => s.SubscriptionPlan)
                .FirstOrDefaultAsync(x => x.ChargingSessionId == id);
        }

        // ============================================================
        // 🔹 Thêm / Cập nhật / Xóa
        // ============================================================
        public async Task AddAsync(ChargingSession session)
        {
            await _context.ChargingSessions.AddAsync(session);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ChargingSession session)
        {
            _context.ChargingSessions.Update(session);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(ChargingSession session)
        {
            _context.ChargingSessions.Remove(session);
            await _context.SaveChangesAsync();
        }
        public IQueryable<ChargingSession> Query()
        {
            return _context.ChargingSessions.AsQueryable();
        }

    }
}
