using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;
using Repositories.Models;
using System.Threading.Tasks;

namespace Repositories.Implementations
{
    public class CompanyRepository : ICompanyRepository
    {
        private readonly ChargeStationContext _context;
        public CompanyRepository(ChargeStationContext context) => _context = context;

        public async Task AddAsync(Company company)
        {
            await _context.Companies.AddAsync(company);
            await _context.SaveChangesAsync();
        }

        public async Task<Company?> GetByTaxCodeAsync(string taxCode)
        {
            return await _context.Companies.FirstOrDefaultAsync(c => c.TaxCode == taxCode);
        }
    }
}
