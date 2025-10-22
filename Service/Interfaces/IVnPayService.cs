using Microsoft.AspNetCore.Http;
using Repositories.DTOs;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IVnPayService
    {
        /// <summary>
        /// Tạo URL thanh toán cho Booking hoặc Invoice.
        /// </summary>
        Task<string> CreatePaymentUrl(PaymentCreateDto dto, string ipAddress, string txnRef);

        /// <summary>
        /// Kiểm tra callback từ VNPay có hợp lệ hay không.
        /// </summary>
        bool ValidateResponse(IQueryCollection vnpParams, out string txnRef);
    }
}
