using Repositories.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface ICustomerRepository
    {
        // 🔹 Lấy tất cả khách hàng
        Task<IEnumerable<Customer>> GetAllAsync();

        // 🔹 Lấy khách hàng theo ID
        Task<Customer?> GetByIdAsync(int id);

        // 🔹 Lấy khách hàng theo AccountId (liên kết 1-1 với Account)
        Task<Customer?> GetByAccountIdAsync(int accountId);

        // 🔹 Thêm khách hàng mới
        Task AddAsync(Customer customer);

        // 🔹 Cập nhật thông tin khách hàng
        Task UpdateAsync(Customer customer);

        // 🔹 Xóa khách hàng
        Task DeleteAsync(Customer customer);

        // 🔹 Lưu thay đổi
        Task SaveChangesAsync();
    }
}
