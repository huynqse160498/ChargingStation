using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;
using Repositories.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories.Implementations
{
    public class PortRepository : IPortRepository
    {
        private readonly ChargeStationContext _context;
        public PortRepository(ChargeStationContext context) => _context = context;

        // ✅ Lấy tất cả port (kèm Charger)
        public async Task<List<Port>> GetAllAsync()
        {
            return await _context.Ports
                .Include(p => p.Charger)
                .AsNoTracking()
                .ToListAsync();
        }

        // ✅ Lấy chi tiết 1 port (kèm Charger)
        public async Task<Port?> GetByIdAsync(int id)
        {
            return await _context.Ports
                .Include(p => p.Charger)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PortId == id);
        }

        // ✅ Thêm mới
        public async Task AddAsync(Port port)
        {
            _context.Ports.Add(port);
            await _context.SaveChangesAsync();
        }

        // ✅ Cập nhật (fix lỗi entity tracking trùng lặp)
        public async Task UpdateAsync(Port port)
        {
            // Gỡ entity trùng trong Local Tracking nếu có
            var local = _context.Ports.Local.FirstOrDefault(e => e.PortId == port.PortId);
            if (local != null)
                _context.Entry(local).State = EntityState.Detached;

            // Gắn lại entity để update
            _context.Entry(port).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        // ✅ Xóa
        public async Task DeleteAsync(Port port)
        {
            _context.Ports.Remove(port);
            await _context.SaveChangesAsync();
        }

        // ✅ Kiểm tra connector trùng trên 1 charger
        public async Task<bool> ExistsConnectorAsync(int chargerId, string connectorType, int? ignoreId = null)
        {
            var q = _context.Ports.AsQueryable()
                .Where(p => p.ChargerId == chargerId && p.ConnectorType == connectorType);
            if (ignoreId.HasValue)
                q = q.Where(p => p.PortId != ignoreId.Value);
            return await q.AnyAsync();
        }

        // ✅ Phân trang
        public async Task<List<Port>> GetPagedAsync(int page, int pageSize, int? chargerId, string? status)
        {
            var q = _context.Ports.Include(p => p.Charger).AsNoTracking().AsQueryable();
            if (chargerId.HasValue)
                q = q.Where(p => p.ChargerId == chargerId.Value);
            if (!string.IsNullOrWhiteSpace(status))
                q = q.Where(p => p.Status == status.Trim());

            return await q.OrderBy(p => p.PortId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        // ✅ Cập nhật trạng thái Port
        public async Task<bool> UpdateStatusAsync(int id, string status)
        {
            var local = _context.Ports.Local.FirstOrDefault(e => e.PortId == id);
            if (local != null)
                _context.Entry(local).State = EntityState.Detached;

            var entity = await _context.Ports.FirstOrDefaultAsync(p => p.PortId == id);
            if (entity == null) return false;

            entity.Status = status;
            entity.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<int> CountAsync(int? chargerId, string? status)
        {
            var q = _context.Ports.AsQueryable();

            if (chargerId.HasValue)
                q = q.Where(p => p.ChargerId == chargerId.Value);

            if (!string.IsNullOrWhiteSpace(status))
                q = q.Where(p => p.Status == status.Trim());

            return await q.CountAsync();
        }
        public IQueryable<Port> Query()
        {
            return _context.Ports.AsQueryable();
        }

        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}
