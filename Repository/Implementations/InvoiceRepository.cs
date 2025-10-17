using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;
using Repositories.Models;

namespace Repositories.Implementations
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly ChargeStationContext _context;

        public InvoiceRepository(ChargeStationContext context)
        {
            _context = context;
        }

        public async Task<List<Invoice>> GetAllAsync()
        {
            return await _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.Subscription)
                .Include(i => i.ChargingSessions)
                .ToListAsync();
        }

        public async Task<Invoice?> GetByIdAsync(int id)
        {
            return await _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.Subscription)
                .Include(i => i.ChargingSessions)
                .FirstOrDefaultAsync(i => i.InvoiceId == id);
        }

        public async Task<Invoice?> GetByCustomerAndMonthAsync(int customerId, int month, int year)
        {
            return await _context.Invoices
                .FirstOrDefaultAsync(i => i.CustomerId == customerId && i.BillingMonth == month && i.BillingYear == year);
        }

        public async Task AddAsync(Invoice invoice)
        {
            await _context.Invoices.AddAsync(invoice);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Invoice invoice)
        {
            _context.Invoices.Update(invoice);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Invoice invoice)
        {
            _context.Invoices.Remove(invoice);
            await _context.SaveChangesAsync();
        }

        // ✅ Dùng để tạo hóa đơn tháng tự động khi session kết thúc
        public async Task<Invoice> GetOrCreateMonthlyInvoiceAsync(int customerId, int month, int year)
        {
            var invoice = await _context.Invoices
                .Include(i => i.ChargingSessions)
                .FirstOrDefaultAsync(i =>
                    i.CustomerId == customerId &&
                    i.BillingMonth == month &&
                    i.BillingYear == year);

            if (invoice == null)
            {
                invoice = new Invoice
                {
                    CustomerId = customerId,
                    BillingMonth = month,
                    BillingYear = year,
                    Status = "Unpaid",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                await _context.Invoices.AddAsync(invoice);
                await _context.SaveChangesAsync();
            }

            return invoice;
        }

        public async Task SaveAsync() => await _context.SaveChangesAsync();
    }
}
