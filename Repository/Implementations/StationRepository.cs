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
    public class StationRepository : IStationRepository
    {
        private readonly ChargeStationContext _context;
        public StationRepository(ChargeStationContext context) { _context = context; }

        public async Task<List<Station>> GetAllAsync()
            => await _context.Stations.AsNoTracking().ToListAsync();

        public async Task<Station?> GetByIdAsync(int id)
            => await _context.Stations.FirstOrDefaultAsync(s => s.StationId == id);

        public async Task AddAsync(Station station)
        {
            _context.Stations.Add(station);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Station station)
        {
            _context.Stations.Update(station);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Station station)
        {
            _context.Stations.Remove(station);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsNameAsync(string stationName, int? ignoreId = null)
        {
            var q = _context.Stations.AsQueryable()
                                     .Where(s => s.StationName == stationName);
            if (ignoreId.HasValue) q = q.Where(s => s.StationId != ignoreId.Value);
            return await q.AnyAsync();
        }

        // ===== NEW: helpers =====
        private static IQueryable<Station> ApplyFilters(IQueryable<Station> q, string? stationName, string? city, string? status)
        {
            if (!string.IsNullOrWhiteSpace(stationName))
                q = q.Where(s => s.StationName.Contains(stationName));
            if (!string.IsNullOrWhiteSpace(city))
                q = q.Where(s => s.City!.Contains(city));
            if (!string.IsNullOrWhiteSpace(status))
                q = q.Where(s => s.Status == status);
            return q;
        }

        public async Task<int> CountAsync(string? stationName, string? city, string? status)
        {
            var q = ApplyFilters(_context.Stations.AsQueryable(), stationName, city, status);
            return await q.CountAsync();
        }

        public async Task<List<Station>> GetPagedAsync(int page, int pageSize, string? stationName, string? city, string? status)
        {
            var q = ApplyFilters(_context.Stations.AsNoTracking(), stationName, city, status);
            return await q.OrderBy(s => s.StationId)
                          .Skip((page - 1) * pageSize)
                          .Take(pageSize)
                          .ToListAsync();
        }

        public async Task<bool> UpdateStatusAsync(int id, string status)
        {
            var entity = await _context.Stations.FirstOrDefaultAsync(s => s.StationId == id);
            if (entity == null) return false;
            entity.Status = status;
            entity.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}
