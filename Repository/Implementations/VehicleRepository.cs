using Microsoft.EntityFrameworkCore;
using Repositories.Models;
using Repositories.Interfaces;
using System;
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
            return await _context.Vehicles
                .AsNoTracking()
                .OrderByDescending(v => v.UpdatedAt ?? v.CreatedAt)
                .ThenBy(v => v.VehicleId)
                .ToListAsync();
        }

        public async Task<Vehicle> GetByIdAsync(int id)
        {
            return await _context.Vehicles.FirstOrDefaultAsync(v => v.VehicleId == id);
        }

        public async Task AddAsync(Vehicle vehicle)
        {
            _context.Vehicles.Add(vehicle);
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
            var normalized = (licensePlate ?? string.Empty).Trim().ToUpperInvariant();
            var q = _context.Vehicles.AsNoTracking();

            q = q.Where(x => (x.LicensePlate ?? string.Empty).ToUpper() == normalized);
            if (ignoreId.HasValue) q = q.Where(x => x.VehicleId != ignoreId.Value);

            return await q.AnyAsync();
        }

        // ========= helpers =========
        private IQueryable<Vehicle> ApplyFilters(IQueryable<Vehicle> q,
            string? licensePlate, string? carMaker, string? model, string? status,
            int? yearFrom, int? yearTo, string? vehicleType = null) // NEW: thêm vehicleType //NEW
        {
            if (!string.IsNullOrWhiteSpace(licensePlate))
            {
                var s = licensePlate.Trim().ToUpperInvariant();
                q = q.Where(v => (v.LicensePlate ?? string.Empty).ToUpper().Contains(s));
            }

            if (!string.IsNullOrWhiteSpace(carMaker))
            {
                var s = carMaker.Trim().ToUpperInvariant();
                q = q.Where(v => (v.CarMaker ?? string.Empty).ToUpper().Contains(s));
            }

            if (!string.IsNullOrWhiteSpace(model))
            {
                var s = model.Trim().ToUpperInvariant();
                q = q.Where(v => (v.Model ?? string.Empty).ToUpper().Contains(s));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                var s = status.Trim();
                q = q.Where(v => v.Status == s);
            }

            if (yearFrom.HasValue)
                q = q.Where(v => v.ManufactureYear >= yearFrom.Value);

            if (yearTo.HasValue)
                q = q.Where(v => v.ManufactureYear <= yearTo.Value);

            if (!string.IsNullOrWhiteSpace(vehicleType))                        // NEW //NEW
            {
                var s = vehicleType.Trim().ToUpperInvariant();                  // NEW //NEW
                q = q.Where(v => (v.VehicleType ?? string.Empty).ToUpper().Contains(s)); // NEW //NEW
            }

            return q;
        }

        public async Task<int> CountAsync(
            string? licensePlate, string? carMaker, string? model, string? status,
            int? yearFrom, int? yearTo, string? vehicleType = null)             // NEW param //NEW
        {
            var q = ApplyFilters(_context.Vehicles.AsNoTracking(),
                                 licensePlate, carMaker, model, status, yearFrom, yearTo, vehicleType); // NEW //NEW
            return await q.CountAsync();
        }

        public async Task<List<Vehicle>> GetPagedAsync(
            int page, int pageSize,
            string? licensePlate, string? carMaker, string? model, string? status,
            int? yearFrom, int? yearTo, string? vehicleType = null)             // NEW param //NEW
        {
            var q = ApplyFilters(_context.Vehicles.AsNoTracking(),
                                 licensePlate, carMaker, model, status, yearFrom, yearTo, vehicleType); // NEW //NEW

            return await q.OrderByDescending(v => v.UpdatedAt ?? v.CreatedAt)
                          .ThenBy(v => v.VehicleId)
                          .Skip((page - 1) * pageSize)
                          .Take(pageSize)
                          .ToListAsync();
        }

        public async Task<bool> UpdateStatusAsync(int id, string status) //NEW
        {
            var entity = await _context.Vehicles.FirstOrDefaultAsync(v => v.VehicleId == id);
            if (entity == null) return false;

            entity.Status = status?.Trim();
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task SaveChangesAsync() //NEW
        {
            await _context.SaveChangesAsync();
        }
    }
}
