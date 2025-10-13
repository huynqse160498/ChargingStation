using Microsoft.AspNetCore.Http;
using Repositories.DTOs;

namespace Services.Interfaces
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(PaymentCreateDto dto, string ipAddress, string txnRef);
        bool ValidateResponse(IQueryCollection vnpParams, out string txnRef);
    }
}
