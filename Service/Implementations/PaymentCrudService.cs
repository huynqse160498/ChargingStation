using Repositories.DTOs;
using Repositories.Interfaces;
using Repositories.Models;
using Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services.Implementations
{
    public class PaymentCrudService : IPaymentCrudService
    {
        private readonly IPaymentRepository _repo;

        public PaymentCrudService(IPaymentRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<PaymentListItemDto>> GetAllAsync()
        {
            var data = await _repo.GetAllAsync();
            return data.Select(p => new PaymentListItemDto
            {
                PaymentId = p.PaymentId,
                BookingId = p.BookingId ?? 0,
                InvoiceId = p.InvoiceId ?? 0,
                SubscriptionId = p.SubscriptionId ?? 0,
                CompanyId = p.CompanyId,
                Amount = p.Amount,
                Method = p.Method,
                Status = p.Status,
                PaidAt = p.PaidAt
            });
        }

        public async Task<PaymentDetailDto?> GetByIdAsync(int id)
        {
            var p = await _repo.GetByIdAsync(id);
            if (p == null) return null;

            return new PaymentDetailDto
            {
                PaymentId = p.PaymentId,
                BookingId = p.BookingId ?? 0,
                InvoiceId = p.InvoiceId ?? 0,
                SubscriptionId = p.SubscriptionId ?? 0,
                CompanyId = p.CompanyId,
                Amount = p.Amount,
                Method = p.Method,
                Status = p.Status,
                PaidAt = p.PaidAt,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            };
        }

        public async Task<bool> UpdateAsync(int id, PaymentUpdateDto dto)
        {
            var payment = await _repo.GetByIdAsync(id);
            if (payment == null) return false;

            if (dto.Amount.HasValue)
                payment.Amount = dto.Amount;
            if (!string.IsNullOrWhiteSpace(dto.Method))
                payment.Method = dto.Method;
            if (!string.IsNullOrWhiteSpace(dto.Status))
                payment.Status = dto.Status;

            payment.UpdatedAt = System.DateTime.Now;

            await _repo.UpdateAsync(payment);
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var payment = await _repo.GetByIdAsync(id);
            if (payment == null) return false;

            await _repo.DeleteAsync(payment);
            return true;
        }
    }
}
