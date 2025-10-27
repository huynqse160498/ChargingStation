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
        public IQueryable<Invoice> Query() => _context.Invoices.AsQueryable(); // ✅ thêm dòng này

        // ============================================================
        // 🔹 Lấy tất cả hóa đơn (FULL thông tin)
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

        // 🔹 Lấy chi tiết hóa đơn theo ID
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
        // 🔹 Lấy hóa đơn theo Customer và tháng
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

        // 🔹 Lấy hóa đơn theo Company và tháng
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
        // 🔹 Thêm / Cập nhật / Xóa cơ bản
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
        // 🔹 Get hoặc Create hóa đơn tháng (logic mới)
        // ============================================================
        public async Task<Invoice> GetOrCreateMonthlyInvoiceAsync(int? customerId, int? companyId, int month, int year)
        {
            // ⚙️ Tìm hóa đơn chưa thanh toán trong tháng
            var invoice = await _context.Invoices
                .Include(i => i.Subscription)
                    .ThenInclude(s => s.SubscriptionPlan)
                .Include(i => i.ChargingSessions)
                .FirstOrDefaultAsync(i =>
                    i.CustomerId == customerId &&
                    i.CompanyId == companyId &&
                    i.BillingMonth == month &&
                    i.BillingYear == year &&
                    i.Status != "Paid");

            // ✅ Nếu chưa có → tạo mới
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
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                await _context.Invoices.AddAsync(invoice);
                await _context.SaveChangesAsync();

                // ⚡ Load lại có include subscription
                invoice = await _context.Invoices
                    .Include(i => i.Subscription)
                        .ThenInclude(s => s.SubscriptionPlan)
                    .FirstOrDefaultAsync(i => i.InvoiceId == invoice.InvoiceId);

                return invoice;
            }

            // ⚠️ Nếu hóa đơn đã thanh toán → tạo hóa đơn mới cho tháng đó
            if (invoice.Status == "Paid")
            {
                var newInvoice = new Invoice
                {
                    CustomerId = customerId,
                    CompanyId = companyId,
                    BillingMonth = month,
                    BillingYear = year,
                    Status = "Unpaid",
                    IsMonthlyInvoice = true,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                await _context.Invoices.AddAsync(newInvoice);
                await _context.SaveChangesAsync();

                newInvoice = await _context.Invoices
                    .Include(i => i.Subscription)
                        .ThenInclude(s => s.SubscriptionPlan)
                    .FirstOrDefaultAsync(i => i.InvoiceId == newInvoice.InvoiceId);

                return newInvoice;
            }

            return invoice;
        }

        // ============================================================
        // 🔹 Tính lại tổng tiền hóa đơn
        // ============================================================
        public async Task RecalculateInvoiceAsync(int invoiceId)
        {
            var invoice = await _context.Invoices
                .Include(i => i.ChargingSessions)
                .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);

            if (invoice == null)
                throw new Exception("Không tìm thấy hóa đơn.");

            // Tổng các phiên sạc
            var subtotal = invoice.ChargingSessions.Sum(s => s.Subtotal ?? 0);
            var tax = subtotal * 0.1M; // VAT 10%
            var adj = invoice.SubscriptionAdjustment ?? 0;

            invoice.Subtotal = subtotal;
            invoice.Tax = tax;
            invoice.Total = subtotal + tax + adj;
            invoice.UpdatedAt = DateTime.Now;

            _context.Invoices.Update(invoice);
            await _context.SaveChangesAsync();
        }

        public async Task SaveAsync() => await _context.SaveChangesAsync();
    }

}
