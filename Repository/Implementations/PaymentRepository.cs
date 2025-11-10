using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;
using Repositories.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories.Implementations
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly ChargeStationContext _context;

        public PaymentRepository(ChargeStationContext context)
        {
            _context = context;
        }

        // GetAll: KHÔNG Include để không rớt các payment vãng lai
        public async Task<List<Payment>> GetAllAsync()
        {
            return await _context.Payments
                .AsNoTracking()
                .OrderByDescending(p => p.PaidAt ?? p.CreatedAt) // tuỳ chọn
                .ToListAsync();
        }

        // GetById: chỉ Include khi cần trả về dữ liệu navigation
        public async Task<Payment?> GetByIdAsync(int id)
        {
            return await _context.Payments
                .Include(p => p.Customer)
                .Include(p => p.Company)
                .Include(p => p.Booking)
                .Include(p => p.Invoice)
                .Include(p => p.Subscription)
                .Include(p => p.ChargingSession)
                .FirstOrDefaultAsync(p => p.PaymentId == id);
        }

        public async Task<Payment> AddAsync(Payment payment)
        {
            await _context.Payments.AddAsync(payment);
            await _context.SaveChangesAsync();
            return payment;
        }

        public async Task<Payment> UpdateAsync(Payment payment)
        {
            _context.Payments.Update(payment);
            await _context.SaveChangesAsync();
            return payment;
        }

        public async Task DeleteAsync(Payment payment)
        {
            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();
        }

        public async Task SaveAsync() => await _context.SaveChangesAsync();

        // (Tuỳ chọn) Lấy theo phiên sạc – tiện test payment vãng lai
        public Task<List<Payment>> GetByChargingSessionAsync(int sessionId)
        {
            return _context.Payments
                .AsNoTracking()
                .Where(p => p.ChargingSessionId == sessionId)
                .OrderByDescending(p => p.PaidAt ?? p.CreatedAt)
                .ToListAsync();
        }
    }
}
