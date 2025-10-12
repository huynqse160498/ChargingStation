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
    public class ChargerRepository : IChargerRepository
    {
        private readonly ChargeStationContext _context;
        public ChargerRepository(ChargeStationContext context) { _context = context; }

        public async Task<List<Charger>> GetAllAsync()
            => await _context.Chargers.AsNoTracking().ToListAsync();

        public async Task<Charger?> GetByIdAsync(int id)
            => await _context.Chargers.FirstOrDefaultAsync(c => c.ChargerId == id);

        public async Task AddAsync(Charger charger)
        {
            _context.Chargers.Add(charger);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Charger charger)
        {
            _context.Chargers.Update(charger);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Charger charger)
        {
            _context.Chargers.Remove(charger);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsCodeAsync(string code, int? ignoreId = null)
        {
            var q = _context.Chargers.AsQueryable().Where(x => x.Code == code);
            if (ignoreId.HasValue) q = q.Where(x => x.ChargerId != ignoreId.Value);
            return await q.AnyAsync();
        }

        // ===== NEW: paging/filter helpers =====
        private static IQueryable<Charger> ApplyFilters(IQueryable<Charger> q,
            int? stationId, string? code, string? type, string? status,
            decimal? minPower, decimal? maxPower)
        {
            if (stationId.HasValue) q = q.Where(c => c.StationId == stationId.Value);
            if (!string.IsNullOrWhiteSpace(code)) q = q.Where(c => c.Code.Contains(code));
            if (!string.IsNullOrWhiteSpace(type)) q = q.Where(c => c.Type == type);
            if (!string.IsNullOrWhiteSpace(status)) q = q.Where(c => c.Status == status);
            if (minPower.HasValue) q = q.Where(c => c.PowerKw >= minPower.Value);
            if (maxPower.HasValue) q = q.Where(c => c.PowerKw <= maxPower.Value);
            return q;
        }

        public Task<int> CountAsync(int? stationId, string? code, string? type, string? status,
                                    decimal? minPower, decimal? maxPower)
        {
            var q = ApplyFilters(_context.Chargers.AsQueryable(), stationId, code, type, status, minPower, maxPower);
            return q.CountAsync();
        }

        public Task<List<Charger>> GetPagedAsync(int page, int pageSize,
                                    int? stationId, string? code, string? type, string? status,
                                    decimal? minPower, decimal? maxPower)
        {
            var q = ApplyFilters(_context.Chargers.AsNoTracking(), stationId, code, type, status, minPower, maxPower);
            return q.OrderBy(c => c.ChargerId)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
        }

        public async Task<bool> UpdateStatusAsync(int id, string status)
        {
            var entity = await _context.Chargers.FirstOrDefaultAsync(c => c.ChargerId == id);
            if (entity == null) return false;
            entity.Status = status;
            entity.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }


        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}
