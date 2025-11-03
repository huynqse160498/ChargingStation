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
    public class ReportRepository : IReportRepository
    {
        private readonly ChargeStationContext _db;
        public ReportRepository(ChargeStationContext db) => _db = db;

        public async Task<Report?> GetByIdAsync(int id)
        {
            return await _db.Set<Report>()
                .Include(r => r.Staff)
                .Include(r => r.Station)
                .Include(r => r.Charger)
                .Include(r => r.Port)
                .FirstOrDefaultAsync(r => r.ReportId == id);
        }

        public async Task<List<Report>> GetByStationAsync(int stationId)
        {
            return await _db.Set<Report>()
                .Include(r => r.Staff)
                .Where(r => r.StationId == stationId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task AddAsync(Report entity)
        {
            _db.Set<Report>().Add(entity);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Report entity)
        {
            _db.Set<Report>().Update(entity);
            await _db.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var e = await _db.Set<Report>().FindAsync(id);
            if (e == null) return false;
            _db.Set<Report>().Remove(e);
            return await _db.SaveChangesAsync() > 0;
        }

        // =============== Paging + Filters ===============
        public async Task<int> CountAsync(
            int? stationId, int? chargerId, string? status, string? severity,
            DateTime? from, DateTime? to)
        {
            var q = ApplyFilters(_db.Set<Report>().AsQueryable(), stationId, chargerId, status, severity, from, to);
            return await q.CountAsync();
        }

        public async Task<List<Report>> GetPagedAsync(
            int page, int pageSize,
            int? stationId, int? chargerId, string? status, string? severity,
            DateTime? from, DateTime? to)
        {
            var q = ApplyFilters(
                        _db.Set<Report>()
                           .Include(r => r.Staff)
                           .Include(r => r.Station)
                           .Include(r => r.Charger)
                           .Include(r => r.Port),
                        stationId, chargerId, status, severity, from, to);

            return await q.OrderByDescending(r => r.CreatedAt)
                          .Skip((page - 1) * pageSize)
                          .Take(pageSize)
                          .ToListAsync();
        }

        private static IQueryable<Report> ApplyFilters(
            IQueryable<Report> q,
            int? stationId, int? chargerId, string? status, string? severity,
            DateTime? from, DateTime? to)
        {
            if (stationId is int sid) q = q.Where(r => r.StationId == sid);
            if (chargerId is int cid) q = q.Where(r => r.ChargerId == cid);
            if (!string.IsNullOrWhiteSpace(status)) q = q.Where(r => r.Status == status);
            if (!string.IsNullOrWhiteSpace(severity)) q = q.Where(r => r.Severity == severity);
            if (from.HasValue) q = q.Where(r => r.CreatedAt >= from.Value);
            if (to.HasValue) q = q.Where(r => r.CreatedAt <= to.Value);

            return q;
        }
    }
}
