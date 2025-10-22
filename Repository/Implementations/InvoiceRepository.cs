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

        // 🔹 Lấy tất cả hóa đơn
        public async Task<List<Invoice>> GetAllAsync()
        {
            return await _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.Company)
                .Include(i => i.Subscription)
                .Include(i => i.ChargingSessions)
                .AsNoTracking()
                .ToListAsync();
        }

        // 🔹 Lấy chi tiết hóa đơn
        public async Task<Invoice?> GetByIdAsync(int id)
        {
            return await _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.Company)
                .Include(i => i.Subscription)
                .Include(i => i.ChargingSessions)
                .FirstOrDefaultAsync(i => i.InvoiceId == id);
        }

        // 🔹 Lấy hóa đơn theo Customer và tháng
        public async Task<Invoice?> GetByCustomerAndMonthAsync(int customerId, int month, int year)
        {
            return await _context.Invoices
                .Include(i => i.ChargingSessions)
                .FirstOrDefaultAsync(i =>
                    i.CustomerId == customerId &&
                    i.BillingMonth == month &&
                    i.BillingYear == year);
        }
        public async Task<Invoice?> GetByCompanyAndMonthAsync(int companyId, int month, int year)
        {
            return await _context.Invoices
                .FirstOrDefaultAsync(i =>
                    i.CompanyId == companyId &&
                    i.BillingMonth == month &&
                    i.BillingYear == year);
        }

        // 🔹 Tạo mới
        public async Task AddAsync(Invoice invoice)
        {
            await _context.Invoices.AddAsync(invoice);
            await _context.SaveChangesAsync();
        }

        // 🔹 Cập nhật
        public async Task UpdateAsync(Invoice invoice)
        {
            _context.Invoices.Update(invoice);
            await _context.SaveChangesAsync();
        }

        // 🔹 Xóa
        public async Task DeleteAsync(Invoice invoice)
        {
            _context.Invoices.Remove(invoice);
            await _context.SaveChangesAsync();
        }

        // 🔹 Tự động lấy hoặc tạo hóa đơn tháng
        public async Task<Invoice> GetOrCreateMonthlyInvoiceAsync(int customerId, int month, int year)
        {
            var invoice = await _context.Invoices
                .Include(i => i.ChargingSessions)
                .FirstOrDefaultAsync(i =>
                 i.CustomerId == customerId &&
                 i.BillingMonth == month &&
                 i.BillingYear == year &&
                 i.Status != "Paid");

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

        // 🔹 Cập nhật tổng hóa đơn (subtotal, tax, total)
        public async Task RecalculateInvoiceAsync(int invoiceId)
        {
            var invoice = await _context.Invoices
                .Include(i => i.ChargingSessions)
                .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);

            if (invoice == null)
                throw new Exception("Không tìm thấy hóa đơn.");

            var subtotal = invoice.ChargingSessions.Sum(s => s.Total ?? 0);
            var subscriptionAdj = invoice.SubscriptionAdjustment ?? 0;
            var tax = subtotal * 0.1M;

            invoice.Subtotal = subtotal;
            invoice.Tax = tax;
            invoice.Total = subtotal + tax + subscriptionAdj;
            invoice.UpdatedAt = DateTime.Now;

            _context.Invoices.Update(invoice);
            await _context.SaveChangesAsync();
        }

        public async Task SaveAsync() => await _context.SaveChangesAsync();
    }
}
