using Repositories.Models;

namespace Repositories.Interfaces
{
    public interface IInvoiceRepository
    {
        Task<List<Invoice>> GetAllAsync();
        Task<Invoice?> GetByIdAsync(int id);
        Task<Invoice?> GetByCustomerAndMonthAsync(int customerId, int month, int year);
        Task<Invoice?> GetByCompanyAndMonthAsync(int companyId, int month, int year);

        Task AddAsync(Invoice invoice);
        Task UpdateAsync(Invoice invoice);
        Task DeleteAsync(Invoice invoice);

        Task<Invoice> GetOrCreateMonthlyInvoiceAsync(int? customerId,int? companyId, int month, int year);
        Task RecalculateInvoiceAsync(int invoiceId);

        Task SaveAsync();
    }
}
