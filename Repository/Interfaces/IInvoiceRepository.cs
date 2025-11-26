using Repositories.Models;
using System.Linq.Expressions;

namespace Repositories.Interfaces
{
    public interface IInvoiceRepository
    {
        Task<List<Invoice>> GetAllAsync();
        Task<Invoice?> GetByIdAsync(int id);
        Task<Invoice?> GetByCustomerAndMonthAsync(int customerId, int month, int year);
        Task<Invoice?> GetByCompanyAndMonthAsync(int companyId, int month, int year);
        IQueryable<Invoice> Query(); // ✅ thêm dòng này
        Task AddAsync(Invoice invoice);
        Task UpdateAsync(Invoice invoice);
        Task DeleteAsync(Invoice invoice);

        Task<Invoice> GetOrCreateMonthlyInvoiceAsync(int? customerId,int? companyId, int month, int year);
        Task RecalculateInvoiceAsync(int invoiceId);
        Task<List<Invoice>> GetAllAsync(Expression<Func<Invoice, bool>> filter);

        Task SaveAsync();
    }
}
