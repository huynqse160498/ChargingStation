using System.Threading.Tasks;
using Repositories.Models;

namespace Repositories.Interfaces
{
    public interface ICompanyRepository
    {
        Task AddAsync(Company company);
        Task<Company?> GetByTaxCodeAsync(string taxCode);
    }
}
