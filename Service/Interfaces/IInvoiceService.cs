using Repositories.DTOs;
using Repositories.Models;

namespace Services.Interfaces
{
    public interface IInvoiceService
    {
        Task<List<Invoice>> GetAllAsync();                  // Lấy toàn bộ hóa đơn (Admin)
        Task<Invoice?> GetByIdAsync(int id);                // Lấy chi tiết 1 hóa đơn

        Task<Invoice> CreateAsync(InvoiceCreateDto dto);    // Admin tạo thủ công
        Task UpdateStatusAsync(InvoiceUpdateStatusDto dto); // Cập nhật trạng thái
        Task RecalculateAsync(int invoiceId);               // Tính lại tổng tiền

        Task<List<Invoice>> GetByCustomerIdAsync(int customerId); // Xem hóa đơn khách hàng
        Task<List<Invoice>> GetByCompanyIdAsync(int companyId);   // Xem hóa đơn công ty

        Task DeleteAsync(int id);                           // Xóa hóa đơn
    }
}
