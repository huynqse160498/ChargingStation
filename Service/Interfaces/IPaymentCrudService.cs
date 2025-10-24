using Repositories.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IPaymentCrudService
    {
        Task<IEnumerable<PaymentListItemDto>> GetAllAsync();
        Task<PaymentDetailDto?> GetByIdAsync(int id);
        Task<bool> UpdateAsync(int id, PaymentUpdateDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
