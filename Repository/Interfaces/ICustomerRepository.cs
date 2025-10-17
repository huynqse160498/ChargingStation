using System.Threading.Tasks;
using Repositories.Models;
using System.Collections.Generic;

namespace Repositories.Interfaces
{
    public interface ICustomerRepository
    {
        Task<Customer> GetByIdAsync(int id);      // ✅ thêm dòng này
        Task<IEnumerable<Customer>> GetAllAsync(); // (tùy chọn)
        void Add(Customer customer);
        void Update(Customer customer);
        void Delete(int id);
    }
}
