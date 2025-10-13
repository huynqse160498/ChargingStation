using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;
using Repositories.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories.Implementations
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly ChargeStationContext _db;

        public PaymentRepository(ChargeStationContext db)
        {
            _db = db;
        }

        public IQueryable<Payment> GetAll() => _db.Payments.AsQueryable();

        public async Task<Payment?> GetByIdAsync(int id) => await _db.Payments.FindAsync(id);

        public async Task AddAsync(Payment entity) => await _db.Payments.AddAsync(entity);

        public async Task SaveAsync() => await _db.SaveChangesAsync();
    }
}
