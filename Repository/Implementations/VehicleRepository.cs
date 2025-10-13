using Microsoft.EntityFrameworkCore;
using Repositories.Models;
using System;
using Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories.Implementations
{
    public class VehicleRepository : IVehicleRepository
    {
        private readonly ChargeStationContext _context;

        public VehicleRepository(ChargeStationContext context)
        {
            _context = context;
        }

        public async Task<List<Vehicle>> GetAllAsync()
        {
            // CHANGED: AsNoTracking + order mới nhất trước
            return await _context.Vehicles
                .AsNoTracking()                                           // CHANGED
                .OrderByDescending(v => v.UpdatedAt ?? v.CreatedAt)      // CHANGED
                .ThenBy(v => v.VehicleId)                                 // CHANGED
                .ToListAsync();
        }

        public async Task<Vehicle> GetByIdAsync(int id)
        {
            // Giữ nguyên chữ ký (Vehicle, có thể null) → service nhớ check null
            return await _context.Vehicles.FirstOrDefaultAsync(v => v.VehicleId == id);
        }

        public async Task AddAsync(Vehicle vehicle)
        {
            _context.Vehicles.Add(vehicle);
            // LƯU Ý: Các field theo DTO (CustomerId, CompanyId, CurrentSoc, ...) đã có trên entity Vehicle
            // nên sẽ được persist bình thường khi SaveChanges.
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Vehicle vehicle)
        {
            _context.Vehicles.Update(vehicle);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Vehicle vehicle)
        {
            _context.Vehicles.Remove(vehicle);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsLicenseAsync(string licensePlate, int? ignoreId = null)
        {
            // CHANGED: chuẩn hoá để so sánh không phân biệt hoa/thường + tránh null
            var normalized = (licensePlate ?? string.Empty).Trim().ToUpperInvariant(); // CHANGED
            var q = _context.Vehicles.AsNoTracking().AsQueryable();                    // CHANGED

            q = q.Where(x => (x.LicensePlate ?? string.Empty).ToUpper() == normalized); // CHANGED
            if (ignoreId.HasValue) q = q.Where(x => x.VehicleId != ignoreId.Value);

            return await q.AnyAsync();
        }

        // ========= NEW: helpers =========
        private IQueryable<Vehicle> ApplyFilters(IQueryable<Vehicle> q,
            string? licensePlate, string? carMaker, string? model, string? status,
            int? yearFrom, int? yearTo)
        {
            // CHANGED: Contains không phân biệt hoa/thường + tránh null
            if (!string.IsNullOrWhiteSpace(licensePlate))
            {
                var s = licensePlate.Trim().ToUpperInvariant();                          // CHANGED
                q = q.Where(v => (v.LicensePlate ?? string.Empty).ToUpper().Contains(s)); // CHANGED
            }

            if (!string.IsNullOrWhiteSpace(carMaker))
            {
                var s = carMaker.Trim().ToUpperInvariant();                              // CHANGED
                q = q.Where(v => (v.CarMaker ?? string.Empty).ToUpper().Contains(s));     // CHANGED
            }

            if (!string.IsNullOrWhiteSpace(model))
            {
                var s = model.Trim().ToUpperInvariant();                                 // CHANGED
                q = q.Where(v => (v.Model ?? string.Empty).ToUpper().Contains(s));        // CHANGED
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                var s = status.Trim();
                q = q.Where(v => v.Status == s);  // exact match như cũ
            }

            if (yearFrom.HasValue)
                q = q.Where(v => v.ManufactureYear >= yearFrom.Value);                   // CHANGED

            if (yearTo.HasValue)
                q = q.Where(v => v.ManufactureYear <= yearTo.Value);                     // CHANGED

            return q;
        }

        public async Task<int> CountAsync(string? licensePlate, string? carMaker, string? model, string? status,
                                          int? yearFrom, int? yearTo)
        {
            // CHANGED: AsNoTracking cho count
            var q = ApplyFilters(_context.Vehicles.AsNoTracking(),                       // CHANGED
                                 licensePlate, carMaker, model, status, yearFrom, yearTo);
            return await q.CountAsync();
        }

        public async Task<List<Vehicle>> GetPagedAsync(int page, int pageSize,
                                                       string? licensePlate, string? carMaker, string? model, string? status,
                                                       int? yearFrom, int? yearTo)
        {
            var q = ApplyFilters(_context.Vehicles.AsNoTracking(),
                                 licensePlate, carMaker, model, status, yearFrom, yearTo);

            // CHANGED: order theo UpdatedAt desc → VehicleId
            return await q.OrderByDescending(v => v.UpdatedAt ?? v.CreatedAt)            // CHANGED
                          .ThenBy(v => v.VehicleId)                                      // CHANGED
                          .Skip((page - 1) * pageSize)
                          .Take(pageSize)
                          .ToListAsync();
        }

        public async Task<bool> UpdateStatusAsync(int id, string status)
        {
            var entity = await _context.Vehicles.FirstOrDefaultAsync(v => v.VehicleId == id);
            if (entity == null) return false;

            entity.Status = status?.Trim();      // CHANGED: trim
            entity.UpdatedAt = DateTime.UtcNow;  // CHANGED: cập nhật mốc thời gian

            await _context.SaveChangesAsync();   // giữ nguyên pattern hiện tại
            return true;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
