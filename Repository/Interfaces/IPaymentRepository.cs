using Repositories.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface IPaymentRepository
    {
        IQueryable<Payment> GetAll();
        Task<Payment?> GetByIdAsync(int id);
        Task AddAsync(Payment entity);
        Task SaveAsync();
    }
}
