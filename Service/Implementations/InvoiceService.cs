using Repositories.DTOs;
using Repositories.Implementations;
using Repositories.Interfaces;
using Repositories.Models;
using Services.Interfaces;

namespace Services.Implementations
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IInvoiceRepository _invoiceRepo;
        private readonly ICustomerRepository _customerRepo;

        public InvoiceService(IInvoiceRepository invoiceRepo, ICustomerRepository customerRepo)
        {
            _invoiceRepo = invoiceRepo;
            _customerRepo = customerRepo;
        }

        public async Task<List<Invoice>> GetAllAsync() => await _invoiceRepo.GetAllAsync();

        public async Task<Invoice?> GetByIdAsync(int id) => await _invoiceRepo.GetByIdAsync(id);

        public async Task<Invoice> CreateAsync(InvoiceCreateDto dto)
        {
            var customer = await _customerRepo.GetByIdAsync(dto.CustomerId)
                ?? throw new Exception("Không tìm thấy khách hàng.");

            var invoice = new Invoice
            {
                CustomerId = dto.CustomerId,
                SubscriptionId = dto.SubscriptionId,
                BillingMonth = dto.BillingMonth,
                BillingYear = dto.BillingYear,
                Status = "Unpaid",
                CreatedAt = DateTime.Now
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

        public async Task DeleteAsync(int id)
        {
            var invoice = await _invoiceRepo.GetByIdAsync(id)
                ?? throw new Exception("Không tìm thấy hóa đơn.");
            await _invoiceRepo.DeleteAsync(invoice);
        }
    }
}
