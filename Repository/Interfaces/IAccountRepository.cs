using Repositories.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface IAccountRepository
    {
        Task<Account?> GetByUserNameAsync(string username);
        Task<Account?> GetByIdAsync(int id);
        Task<List<Account>> GetAllAsync();
        Task AddAsync(Account account);
        Task UpdateAsync(Account account);
        Task DeleteAsync(Account account);
        Task SaveChangesAsync();
    }
}
