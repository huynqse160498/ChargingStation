using Repositories.DTOs;
using Repositories.Models;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IAuthService
    {
        Task<string> RegisterAsync(RegisterDto dto);
        Task<object> LoginAsync(LoginDto dto);
        Task<IEnumerable<Account>> GetAllAsync();
        Task<Account> GetByIdAsync(int id);
        Task<bool> DeleteAsync(int id);
        Task<bool> ChangeRoleAsync(int accountId, string newRole);
        Task<bool> ChangeStatusAsync(int accountId, string newStatus);

    }
}
