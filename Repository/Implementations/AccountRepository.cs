using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;
using Repositories.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories.Implementations
{
    public class AccountRepository : IAccountRepository
    {
        private readonly ChargeStationContext _context;

        public AccountRepository(ChargeStationContext context)
        {
            _context = context;
        }

        public async Task<Account?> GetByUserNameAsync(string username)
        {
            return await _context.Accounts
                .Include(a => a.Customers)
                .Include(a => a.Company)
                .FirstOrDefaultAsync(a => a.UserName == username);
        }

        public async Task<Account?> GetByIdAsync(int id)
        {
            return await _context.Accounts
                .Include(a => a.Customers)
                .Include(a => a.Company)
                .ThenInclude(c => c.Subscriptions)           // ✅ thêm dòng này
                .FirstOrDefaultAsync(a => a.AccountId == id);
        }

        public async Task<List<Account>> GetAllAsync()
        {
            return await _context.Accounts
                .Include(a => a.Customers)
                .Include(a => a.Company)
                .ThenInclude(c => c.Subscriptions)
                .ToListAsync();
        }

        public async Task AddAsync(Account account)
        {
            await _context.Accounts.AddAsync(account);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Account account)
        {
            if (!_context.Accounts.Any(a => a.AccountId == account.AccountId))
                throw new KeyNotFoundException("Không tìm thấy tài khoản để cập nhật.");

            _context.Entry(account).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Account account)
        {
            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
