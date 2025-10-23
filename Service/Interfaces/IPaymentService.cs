using Microsoft.AspNetCore.Http;
using Repositories.DTOs;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IPaymentService
    {
       
        Task<string> CreatePaymentUrl(PaymentCreateDto dto, string ipAddress);
        Task<string> CreateSubscriptionPaymentUrl(int subscriptionId, string ipAddress);

        Task<string> HandleCallbackAsync(IQueryCollection query);
    }
}
