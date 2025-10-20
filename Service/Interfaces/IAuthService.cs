using Microsoft.AspNetCore.Http;
using Repositories.DTOs;
using Repositories.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IAuthService
    {
        Task<string> RegisterAsync(RegisterDto dto);
        Task<string> RegisterCompanyAsync(RegisterCompanyDto dto);
        Task<object> LoginAsync(LoginDto dto);
        Task<IEnumerable<Account>> GetAllAsync();
        Task<Account> GetByIdAsync(int id);
        Task<bool> DeleteAsync(int id);
        Task<bool> ChangeRoleAsync(int accountId, string newRole);
        Task<bool> ChangeStatusAsync(int accountId, string newStatus);
        Task<bool> UpdateCustomerAsync(UpdateCustomerDto dto);
        Task<bool> UpdateCompanyAsync(UpdateCompanyDto dto);
        Task<string> UpdateAvatarAsync(int accountId, IFormFile file);
    }
}
