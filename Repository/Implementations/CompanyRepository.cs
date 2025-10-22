using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;
using Repositories.Models;

namespace Repositories.Implementations
{
    public class CompanyRepository : ICompanyRepository
    {
        private readonly ChargeStationContext _context;
        public CompanyRepository(ChargeStationContext context) => _context = context;

        public async Task<List<Company>> GetAllAsync() => await _context.Companies.ToListAsync();

        public async Task<Company?> GetByIdAsync(int id) =>
            await _context.Companies.FirstOrDefaultAsync(c => c.CompanyId == id);
        public async Task<Company?> GetByTaxCodeAsync(string taxCode) =>
            await _context.Companies.FirstOrDefaultAsync(c => c.TaxCode == taxCode);
        public async Task AddAsync(Company company)
        {
            _context.Companies.Add(company);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Company company)
        {
            _context.Companies.Update(company);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Company company)
        {
            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();
        }

        public async Task SaveAsync() => await _context.SaveChangesAsync();
    }
}
