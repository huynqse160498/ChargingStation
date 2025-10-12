using Microsoft.EntityFrameworkCore;
using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Implementations
{
    public class VehicleRepository
    {
        private readonly ChargeStationContext _context;

        public VehicleRepository(ChargeStationContext context)
        {
            _context = context;
        }

        public async Task<List<Vehicle>> GetAllAsync()
        {
            return await _context.Vehicles.AsNoTracking().ToListAsync();
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
            var q = _context.Vehicles.AsQueryable().Where(x => x.LicensePlate == licensePlate);
            if (ignoreId.HasValue) q = q.Where(x => x.VehicleId != ignoreId.Value);
            return await q.AnyAsync();
        }


        // ========= NEW: helpers =========
        private IQueryable<Vehicle> ApplyFilters(IQueryable<Vehicle> q,
            string? licensePlate, string? carMaker, string? model, string? status,
            int? yearFrom, int? yearTo)
        {
            if (!string.IsNullOrWhiteSpace(licensePlate))
                q = q.Where(v => v.LicensePlate.Contains(licensePlate));
            if (!string.IsNullOrWhiteSpace(carMaker))
                q = q.Where(v => v.CarMaker.Contains(carMaker));
            if (!string.IsNullOrWhiteSpace(model))
                q = q.Where(v => v.Model.Contains(model));
            if (!string.IsNullOrWhiteSpace(status))
                q = q.Where(v => v.Status == status);
            if (yearFrom.HasValue)
                q = q.Where(v => v.ManufactureYear >= yearFrom);
            if (yearTo.HasValue)
                q = q.Where(v => v.ManufactureYear <= yearTo);
            return q;
        }

        public async Task<int> CountAsync(string? licensePlate, string? carMaker, string? model, string? status,
                                          int? yearFrom, int? yearTo)
        {
            var q = ApplyFilters(_context.Vehicles.AsQueryable(),
                                 licensePlate, carMaker, model, status, yearFrom, yearTo);
            return await q.CountAsync();
        }

        public async Task<List<Vehicle>> GetPagedAsync(int page, int pageSize,
                                                       string? licensePlate, string? carMaker, string? model, string? status,
                                                       int? yearFrom, int? yearTo)
        {
            var q = ApplyFilters(_context.Vehicles.AsNoTracking(),
                                 licensePlate, carMaker, model, status, yearFrom, yearTo);
            return await q.OrderBy(v => v.VehicleId)
                          .Skip((page - 1) * pageSize)
                          .Take(pageSize)
                          .ToListAsync();
        }

        public async Task<bool> UpdateStatusAsync(int id, string status)
        {
            var entity = await _context.Vehicles.FirstOrDefaultAsync(v => v.VehicleId == id);
            if (entity == null) return false;
            entity.Status = status;
            entity.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
