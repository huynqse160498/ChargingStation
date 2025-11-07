using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;
using Repositories.Models;
using System.Linq.Expressions;

namespace Repositories.Implementations
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly ChargeStationContext _context;

        public InvoiceRepository(ChargeStationContext context)
        {
            _context = context;
        }

        public IQueryable<Invoice> Query() => _context.Invoices.AsQueryable();

        // ============================================================
        // 🔹 GET ALL (full info)
        // ============================================================
        public async Task<List<Invoice>> GetAllAsync()
        {
            return await _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.Company)
                .Include(i => i.Subscription)
                    .ThenInclude(s => s.SubscriptionPlan)
                .Include(i => i.ChargingSessions)
                .AsNoTracking()
                .ToListAsync();
        }

        // ============================================================
        // 🔹 Get By Id
        // ============================================================
        public async Task<Invoice?> GetByIdAsync(int id)
        {
            return await _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.Company)
                .Include(i => i.Subscription)
                    .ThenInclude(s => s.SubscriptionPlan)
                .Include(i => i.ChargingSessions)
                .FirstOrDefaultAsync(i => i.InvoiceId == id);
        }

        // ============================================================
        // 🔹 Get invoice by customer + month
        // ============================================================
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
                .Include(i => i.ChargingSessions)
                .FirstOrDefaultAsync(i =>
                    i.CompanyId == companyId &&
                    i.BillingMonth == month &&
                    i.BillingYear == year);
        }

        // ============================================================
        // 🔹 CREATE / UPDATE / DELETE
        // ============================================================
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

        // ============================================================
        // ✅✅ FIXED — Get or Create monthly invoice
        // ============================================================
        public async Task<Invoice> GetOrCreateMonthlyInvoiceAsync(
            int? customerId,
            int? companyId,
            int month,
            int year)
        {
            var now = DateTime.UtcNow.AddHours(7);

            // ✅ Find existing unpaid invoice
            var invoice = await _context.Invoices
                .Include(i => i.Subscription)
                    .ThenInclude(s => s.SubscriptionPlan)
                .Include(i => i.ChargingSessions)
                .FirstOrDefaultAsync(i =>
                    (
                        (customerId != null && i.CustomerId == customerId) ||
                        (companyId != null && i.CompanyId == companyId)
                    ) &&
                    i.BillingMonth == month &&
                    i.BillingYear == year &&
                    i.IsMonthlyInvoice &&
                    i.Status != "Paid"
                );

            // ✅ Create new if none found
            if (invoice == null)
            {
                invoice = new Invoice
                {
                    CustomerId = customerId,
                    CompanyId = companyId,
                    BillingMonth = month,
                    BillingYear = year,
                    Status = "Unpaid",
                    IsMonthlyInvoice = true,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                await _context.Invoices.AddAsync(invoice);
                await _context.SaveChangesAsync();
            }

            return invoice!;
        }

        // ============================================================
        // ✅ Recalculate invoice total when needed
        // ============================================================
        public async Task RecalculateInvoiceAsync(int invoiceId)
        {
            var invoice = await _context.Invoices
                .Include(i => i.ChargingSessions)
                .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId)
                ?? throw new Exception("Không tìm thấy hóa đơn.");

            invoice.ChargingSessions ??= new List<ChargingSession>();

            var subtotal = invoice.ChargingSessions.Sum(s => s.Subtotal ?? 0);
            var tax = subtotal * 0.1M; // VAT 10%
            var adj = invoice.SubscriptionAdjustment ?? 0;

            invoice.Subtotal = subtotal;
            invoice.Tax = tax;
            invoice.Total = subtotal + tax + adj;
            invoice.UpdatedAt = DateTime.UtcNow.AddHours(7);

            _context.Invoices.Update(invoice);
            await _context.SaveChangesAsync();
        }

        // ============================================================
        // 🔹 GetAll + filter
        // ============================================================
        public async Task<List<Invoice>> GetAllAsync(Expression<Func<Invoice, bool>> filter)
        {
            return await _context.Invoices.Where(filter).ToListAsync();
        }

        public async Task SaveAsync() => await _context.SaveChangesAsync();
    }
}
