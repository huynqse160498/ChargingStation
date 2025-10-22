using Repositories.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface ICompanyRepository
    {
        Task<List<Company>> GetAllAsync();
        Task<Company?> GetByIdAsync(int id); // 🟢 thêm dòng này
        Task AddAsync(Company company);
        Task UpdateAsync(Company company);
        Task DeleteAsync(Company company);
        Task SaveAsync();
        Task<Company?> GetByTaxCodeAsync(string taxCode); // 👈 Thêm dòng này

    }
}
