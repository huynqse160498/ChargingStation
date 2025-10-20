using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;
using Repositories.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories.Implementations
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly ChargeStationContext _context;

        public CustomerRepository(ChargeStationContext context)
        {
            _context = context;
        }

        // 🔹 Lấy danh sách tất cả khách hàng (bao gồm Account và Company)
        public async Task<IEnumerable<Customer>> GetAllAsync()
        {
            return await _context.Customers
                .Include(c => c.Account)
                .Include(c => c.Company)
                .ToListAsync();
        }

        // 🔹 Lấy khách hàng theo ID
        public async Task<Customer?> GetByIdAsync(int id)
        {
            return await _context.Customers
                .Include(c => c.Account)
                .Include(c => c.Company)
                .FirstOrDefaultAsync(c => c.CustomerId == id);
        }

        // 🔹 Lấy khách hàng theo AccountId
        public async Task<Customer?> GetByAccountIdAsync(int accountId)
        {
            return await _context.Customers
                .Include(c => c.Account)
                .Include(c => c.Company)
                .FirstOrDefaultAsync(c => c.AccountId == accountId);
        }

        // 🔹 Thêm mới khách hàng
        public async Task AddAsync(Customer customer)
        {
            await _context.Customers.AddAsync(customer);
            await _context.SaveChangesAsync();
        }

        // 🔹 Cập nhật khách hàng
        public async Task UpdateAsync(Customer customer)
        {
            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();
        }

        // 🔹 Xóa khách hàng
        public async Task DeleteAsync(Customer customer)
        {
            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
        }

        // 🔹 Lưu thay đổi thủ công (nếu cần)
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
