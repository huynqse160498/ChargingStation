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
    public class PortRepository : IPortRepository
    {
        private readonly ChargeStationContext _context;
        public PortRepository(ChargeStationContext context) { _context = context; }

        public async Task<List<Port>> GetAllAsync()
            => await _context.Ports.AsNoTracking().ToListAsync();

        public async Task<Port?> GetByIdAsync(int id)
            => await _context.Ports.FirstOrDefaultAsync(p => p.PortId == id);

        public async Task AddAsync(Port port)
        {
            _context.Ports.Add(port);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Port port)
        {
            _context.Ports.Update(port);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Port port)
        {
            _context.Ports.Remove(port);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsConnectorAsync(int chargerId, string connectorType, int? ignoreId = null)
        {
            var q = _context.Ports.AsQueryable()
                                  .Where(p => p.ChargerId == chargerId && p.ConnectorType == connectorType);
            if (ignoreId.HasValue) q = q.Where(p => p.PortId != ignoreId.Value);
            return await q.AnyAsync();
        }

        // NEW
        public async Task<int> CountAsync(int? chargerId, string? status)
        {
            var q = _context.Ports.AsQueryable();
            if (chargerId.HasValue) q = q.Where(p => p.ChargerId == chargerId.Value);
            if (!string.IsNullOrWhiteSpace(status)) q = q.Where(p => p.Status == status);
            return await q.CountAsync();
        }

        // NEW
        public async Task<List<Port>> GetPagedAsync(int page, int pageSize, int? chargerId, string? status)
        {
            var q = _context.Ports.AsNoTracking().AsQueryable();
            if (chargerId.HasValue) q = q.Where(p => p.ChargerId == chargerId.Value);
            if (!string.IsNullOrWhiteSpace(status)) q = q.Where(p => p.Status == status);

            return await q.OrderBy(p => p.PortId)
                          .Skip((page - 1) * pageSize)
                          .Take(pageSize)
                          .ToListAsync();
        }

        // NEW
        public async Task<bool> UpdateStatusAsync(int id, string status)
        {
            var entity = await _context.Ports.FirstOrDefaultAsync(p => p.PortId == id);
            if (entity == null) return false;
            entity.Status = status;
            entity.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}
