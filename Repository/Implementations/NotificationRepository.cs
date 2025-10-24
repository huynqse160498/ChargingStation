using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;
using Repositories.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories.Implementations
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly ChargeStationContext _context;

        public NotificationRepository(ChargeStationContext context)
        {
            _context = context;
        }

        public async Task<List<Notification>> GetAllAsync()
        {
            return await _context.Notifications
                .Include(n => n.Customer)
                .Include(n => n.Company)
                .Include(n => n.Booking)
                .Include(n => n.Invoice)
                .Include(n => n.Subscription)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Notification>> GetByCustomerAsync(int customerId)
        {
            return await _context.Notifications
                .Where(n => n.CustomerId == customerId)
                .Include(n => n.Booking)
                .Include(n => n.Invoice)
                .Include(n => n.Subscription)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Notification>> GetByCompanyAsync(int companyId)
        {
            return await _context.Notifications
                .Where(n => n.CompanyId == companyId)
                .Include(n => n.Booking)
                .Include(n => n.Invoice)
                .Include(n => n.Subscription)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<Notification?> GetByIdAsync(int id)
        {
            return await _context.Notifications
                .Include(n => n.Customer)
                .Include(n => n.Company)
                .Include(n => n.Booking)
                .Include(n => n.Invoice)
                .Include(n => n.Subscription)
                .FirstOrDefaultAsync(n => n.NotificationId == id);
        }

        public async Task<Notification> AddAsync(Notification entity)
        {
            await _context.Notifications.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<Notification> UpdateAsync(Notification entity)
        {
            _context.Notifications.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task DeleteAsync(Notification entity)
        {
            _context.Notifications.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
