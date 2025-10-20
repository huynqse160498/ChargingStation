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

        // ======================= [CRUD] =======================
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


        // ======================= [FILTER + PAGING] =======================
        private static IQueryable<Charger> ApplyFilters(IQueryable<Charger> q,
            int? stationId, string? code, string? type, string? status,
            decimal? minPower, decimal? maxPower)
        {
            if (stationId.HasValue) q = q.Where(c => c.StationId == stationId.Value);
            if (!string.IsNullOrWhiteSpace(code)) q = q.Where(c => c.Code!.Contains(code));
            if (!string.IsNullOrWhiteSpace(type)) q = q.Where(c => c.Type == type);
            if (!string.IsNullOrWhiteSpace(status))
            {
                var s = status.Trim();
                // chỉ nhận 3 trạng thái hợp lệ
                if (s == "Online" || s == "Offline" || s == "OutOfOrder")
                    q = q.Where(c => c.Status == s);
            }
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

        // ======================= [INCLUDE PORTS] =======================
        public Task<Charger?> GetByIdWithPortsAsync(int id)
            => _context.Chargers
                       .AsNoTracking()
                       .Include(c => c.Ports)
                       .FirstOrDefaultAsync(c => c.ChargerId == id);


        public Task<List<Charger>> GetPagedWithPortsAsync(
            int page, int pageSize, int? stationId = null, string? status = null)
        {
            var q = _context.Chargers
                            .AsNoTracking()
                            .Include(c => c.Ports)
                            .AsQueryable();


            if (stationId.HasValue) q = q.Where(c => c.StationId == stationId.Value);
            if (!string.IsNullOrWhiteSpace(status))
            {
                var s = status.Trim();
                if (s == "Online" || s == "Offline" || s == "OutOfOrder")
                    q = q.Where(c => c.Status == s);
            }
            return q.OrderBy(c => c.ChargerId)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
        }

        // ======================= [STATUS UPDATE] =======================
        public async Task<bool> UpdateStatusAsync(int id, string status)
        {
            var entity = await _context.Chargers.FirstOrDefaultAsync(c => c.ChargerId == id);
            if (entity == null) return false;

            // normalize: chỉ 3 trạng thái, mặc định Online
            entity.Status = status == "Offline"
                          ? "Offline"
                          : (status == "OutOfOrder" ? "OutOfOrder" : "Online");

            entity.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public Task SaveChangesAsync() => _context.SaveChangesAsync();
    }
}
