using Repositories.DTOs;
using Repositories.Interfaces;
using Repositories.Models;
using Services.Interfaces;

namespace Services.Implementations
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IInvoiceRepository _invoiceRepo;
        private readonly ICustomerRepository _customerRepo;
        private readonly ICompanyRepository _companyRepo;

        public InvoiceService(IInvoiceRepository invoiceRepo, ICustomerRepository customerRepo, ICompanyRepository companyRepo)
        {
            _invoiceRepo = invoiceRepo;
            _customerRepo = customerRepo;
            _companyRepo = companyRepo;
        }

        public async Task<List<Invoice>> GetAllAsync() => await _invoiceRepo.GetAllAsync();

        public async Task<Invoice?> GetByIdAsync(int id) => await _invoiceRepo.GetByIdAsync(id);

        public async Task<Invoice> CreateAsync(InvoiceCreateDto dto)
        {
            if (dto.CustomerId == null && dto.CompanyId == null)
                throw new Exception("Phải chọn Customer hoặc Company để tạo hóa đơn.");

            if (dto.CustomerId != null)
                _ = await _customerRepo.GetByIdAsync(dto.CustomerId.Value)
                    ?? throw new Exception("Không tìm thấy khách hàng.");

            if (dto.CompanyId != null)
                _ = await _companyRepo.GetByIdAsync(dto.CompanyId.Value)
                    ?? throw new Exception("Không tìm thấy công ty.");

            var invoice = new Invoice
            {
                CustomerId = dto.CustomerId,
                CompanyId = dto.CompanyId,
                SubscriptionId = dto.SubscriptionId,
                BillingMonth = dto.BillingMonth,
                BillingYear = dto.BillingYear,
                Subtotal = dto.Subtotal,
                Tax = dto.Tax ?? (dto.Subtotal * 0.1M),
                Total = dto.Total ?? (dto.Subtotal * 1.1M),
                Status = "Unpaid",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };


            await _invoiceRepo.AddAsync(invoice);
            return invoice;
        }

        public async Task UpdateStatusAsync(InvoiceUpdateStatusDto dto)
        {
            var invoice = await _invoiceRepo.GetByIdAsync(dto.InvoiceId)
                ?? throw new Exception("Không tìm thấy hóa đơn.");

            invoice.Status = dto.Status;
            invoice.UpdatedAt = DateTime.Now;
            await _invoiceRepo.UpdateAsync(invoice);
        }

        public async Task RecalculateAsync(int invoiceId)
        {
            await _invoiceRepo.RecalculateInvoiceAsync(invoiceId);
        }

        public async Task<List<Invoice>> GetByCustomerIdAsync(int customerId)
        {
            return (await _invoiceRepo.GetAllAsync()).Where(i => i.CustomerId == customerId).ToList();
        }

        public async Task<List<Invoice>> GetByCompanyIdAsync(int companyId)
        {
            return (await _invoiceRepo.GetAllAsync()).Where(i => i.CompanyId == companyId).ToList();
        }

        public async Task DeleteAsync(int id)
        {
            var invoice = await _invoiceRepo.GetByIdAsync(id)
                ?? throw new Exception("Không tìm thấy hóa đơn.");
            await _invoiceRepo.DeleteAsync(invoice);
        }
    }
}
