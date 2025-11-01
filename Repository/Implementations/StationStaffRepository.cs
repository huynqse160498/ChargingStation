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
    public class StationStaffRepository : IStationStaffRepository
    {
        private readonly ChargeStationContext _context;
        public StationStaffRepository(ChargeStationContext context) => _context = context;

        public async Task<List<StationStaff>> GetByStationAsync(int stationId)
        {
            return await _context.Set<StationStaff>()
                .Include(ss => ss.Staff)    // cần thông tin Account cho UI
                .Where(ss => ss.StationId == stationId)
                .OrderBy(ss => ss.StaffId)
                .ToListAsync();
        }

        public async Task<StationStaff?> GetAsync(int stationId, int staffId)
        {
            return await _context.Set<StationStaff>()
                .Include(ss => ss.Staff)
                .FirstOrDefaultAsync(ss => ss.StationId == stationId && ss.StaffId == staffId);
        }

        public async Task AddAsync(StationStaff entity)
        {
            _context.Set<StationStaff>().Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int stationId, int staffId)
        {
            var e = await _context.Set<StationStaff>().FindAsync(stationId, staffId);
            if (e == null) return false;
            _context.Set<StationStaff>().Remove(e);
            return await _context.SaveChangesAsync() > 0;
        }

        public Task<bool> ExistsAsync(int stationId, int staffId)
        {
            return _context.Set<StationStaff>()
                .AnyAsync(ss => ss.StationId == stationId && ss.StaffId == staffId);
        }
    }
}
