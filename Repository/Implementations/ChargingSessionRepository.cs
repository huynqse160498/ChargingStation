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

        public async Task<List<ChargingSession>> GetAllAsync()
        {
            return await _context.ChargingSessions
                .Include(x => x.PricingRule)
                .Include(x => x.Customer)
                .Include(x => x.Vehicle)
                .Include(x => x.Port)
                .ToListAsync();
        }

        public async Task<ChargingSession?> GetByIdAsync(int id)
        {
            return await _context.ChargingSessions
                .Include(x => x.PricingRule)
                .Include(x => x.Customer)
                .Include(x => x.Vehicle)
                .Include(x => x.Port)
                .FirstOrDefaultAsync(x => x.ChargingSessionId == id);
        }

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
    }
}
