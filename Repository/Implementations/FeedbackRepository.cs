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
    public class FeedbackRepository : IFeedbackRepository
    {
        private readonly ChargeStationContext _db;
        public FeedbackRepository(ChargeStationContext db) => _db = db;

        public async Task<Feedback?> GetByIdAsync(int id)
        {
            return await _db.Set<Feedback>()
                .Include(f => f.Customer)
                .Include(f => f.Station)
                .Include(f => f.Charger)
                .Include(f => f.Port)
                .FirstOrDefaultAsync(f => f.FeedbackId == id);
        }

        public async Task<List<Feedback>> GetByStationAsync(int stationId)
        {
            return await _db.Set<Feedback>()
                .Include(f => f.Customer)
                .Where(f => f.StationId == stationId)
                .OrderByDescending(f => f.FeedbackId)
                .ToListAsync();
        }

        public async Task AddAsync(Feedback entity)
        {
            _db.Set<Feedback>().Add(entity);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Feedback entity)
        {
            _db.Set<Feedback>().Update(entity);
            await _db.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var e = await _db.Set<Feedback>().FindAsync(id);
            if (e == null) return false;
            _db.Set<Feedback>().Remove(e);
            return await _db.SaveChangesAsync() > 0;
        }

        // -------- optional: paging/filter ----------
        public async Task<int> CountAsync(int? stationId, int? customerId, int? rating)
        {
            var q = _db.Set<Feedback>().AsQueryable();
            if (stationId is int s) q = q.Where(f => f.StationId == s);
            if (customerId is int c) q = q.Where(f => f.CustomerId == c);
            if (rating is int r) q = q.Where(f => f.Rating == r);
            return await q.CountAsync();
        }

        public async Task<List<Feedback>> GetPagedAsync(int page, int pageSize, int? stationId, int? customerId, int? rating)
        {
            var q = _db.Set<Feedback>()
                .Include(f => f.Customer)
                .Include(f => f.Station)
                .Include(f => f.Charger)
                .Include(f => f.Port)
                .AsQueryable();

            if (stationId is int s) q = q.Where(f => f.StationId == s);
            if (customerId is int c) q = q.Where(f => f.CustomerId == c);
            if (rating is int r) q = q.Where(f => f.Rating == r);

            return await q
                .OrderByDescending(f => f.FeedbackId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}
