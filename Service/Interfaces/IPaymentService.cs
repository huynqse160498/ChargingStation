using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Repositories.DTOs;

namespace Services.Interfaces
{
    public interface IPaymentService
    {
   
        string CreatePaymentUrl(PaymentCreateDto dto, string ipAddress);


        Task<string> HandleCallbackAsync(IQueryCollection query);
    }
}
