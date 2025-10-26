using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Repositories.DTOs;
using Repositories.Interfaces;
using Repositories.Models;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IS3Service _s3Service;
        private readonly ChargeStationContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly IPasswordHasher<Account> _passwordHasher;

        public AuthService(
            IAccountRepository accountRepository,
            ICompanyRepository companyRepository,
            IS3Service s3Service,
            ChargeStationContext context,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration)
        {
            _accountRepository = accountRepository;
            _companyRepository = companyRepository;
            _s3Service = s3Service;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _passwordHasher = new PasswordHasher<Account>();
        }

        // ------------------- Đăng ký Customer -------------------
        public async Task<string> RegisterAsync(RegisterDto dto)
        {
            var existingUser = await _accountRepository.GetByUserNameAsync(dto.UserName);
            if (existingUser != null)
                return "Tên đăng nhập đã tồn tại";

            var account = new Account
            {
                UserName = dto.UserName,
                Role = "Customer",
                CreatedAt = DateTime.Now,
                Status = "Active"
            };

            account.PassWord = _passwordHasher.HashPassword(account, dto.Password);
            await _accountRepository.AddAsync(account);
            await _context.SaveChangesAsync();

            var customer = new Customer
            {
                AccountId = account.AccountId,
                FullName = dto.FullName ?? dto.UserName,
                Phone = dto.Phone,
                CreatedAt = DateTime.Now,
                Status = "Active"
            };

            await _context.Customers.AddAsync(customer);
            await _context.SaveChangesAsync();

            return "Đăng ký thành công";
        }

        // ------------------- Đăng ký Company -------------------
        public async Task<string> RegisterCompanyAsync(RegisterCompanyDto dto)
        {
            var existingUser = await _accountRepository.GetByUserNameAsync(dto.UserName);
            if (existingUser != null)
                return "Tên đăng nhập đã tồn tại";

            var existingCompany = await _companyRepository.GetByTaxCodeAsync(dto.TaxCode);
            if (existingCompany != null)
                return "Mã số thuế đã được sử dụng";

            var account = new Account
            {
                UserName = dto.UserName,
                Role = "Company",
                CreatedAt = DateTime.Now,
                Status = "Active"
            };
            account.PassWord = _passwordHasher.HashPassword(account, dto.Password);

            await _accountRepository.AddAsync(account);
            await _context.SaveChangesAsync();

            var company = new Company
            {
                AccountId = account.AccountId,
                Name = dto.CompanyName,
                TaxCode = dto.TaxCode,
                Email = dto.CompanyEmail,
                Phone = dto.CompanyPhone,
                Address = dto.Address,
                ImageUrl = dto.ImageUrl,
                Status = "Active",
                CreatedAt = DateTime.Now
            };

            await _companyRepository.AddAsync(company);
            await _context.SaveChangesAsync();

            return "Đăng ký doanh nghiệp thành công";
        }

        // ------------------- Đăng nhập -------------------
        public async Task<object> LoginAsync(LoginDto dto)
        {
            var user = await _accountRepository.GetByUserNameAsync(dto.UserName);
            if (user == null)
                return new { Success = false, Message = "Tài khoản không tồn tại" };

            var result = _passwordHasher.VerifyHashedPassword(user, user.PassWord, dto.Password);
            if (result == PasswordVerificationResult.Failed)
                return new { Success = false, Message = "Mật khẩu không đúng" };

            if (!user.Status.Equals("Active", StringComparison.OrdinalIgnoreCase))
                return new { Success = false, Message = "Tài khoản đã bị khóa" };

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.AccountId.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(3),
                signingCredentials: creds
            );

            return new
            {
                Success = true,
                Message = "Đăng nhập thành công",
                Token = new JwtSecurityTokenHandler().WriteToken(token)
            };
        }

        // ------------------- CRUD tài khoản -------------------
        public async Task<IEnumerable<Account>> GetAllAsync()
        {
            return await _context.Accounts
                .Include(a => a.Company)   // ✅ thêm dòng này
                .Include(a => a.Customers) // để load cả Customer nếu là cá nhân
                .ToListAsync();
        }

        public async Task<Account> GetByIdAsync(int id)
        {
            return await _context.Accounts
                .Include(a => a.Company)   // ✅ thêm dòng này
                .Include(a => a.Customers) // vẫn giữ dòng này
                .FirstOrDefaultAsync(a => a.AccountId == id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var account = await _accountRepository.GetByIdAsync(id);
            if (account == null) throw new KeyNotFoundException("Không tìm thấy tài khoản.");
            await _accountRepository.DeleteAsync(account);
            return true;
        }

        public async Task<bool> ChangeRoleAsync(int accountId, string newRole)
        {
            var currentUser = _httpContextAccessor.HttpContext?.User;
            if (currentUser == null) throw new UnauthorizedAccessException("Người dùng chưa đăng nhập.");

            var currentRole = currentUser.FindFirst(ClaimTypes.Role)?.Value;
            if (currentRole == null || !currentRole.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException("Chỉ Admin mới có quyền đổi vai trò.");

            var account = await _accountRepository.GetByIdAsync(accountId);
            if (account == null) throw new KeyNotFoundException("Không tìm thấy tài khoản.");

            account.Role = newRole;
            await _accountRepository.UpdateAsync(account);
            return true;
        }

        public async Task<bool> ChangeStatusAsync(int accountId, string newStatus)
        {
            var account = await _accountRepository.GetByIdAsync(accountId);
            if (account == null) throw new KeyNotFoundException("Không tìm thấy tài khoản.");

            account.Status = newStatus;
            await _accountRepository.UpdateAsync(account);
            return true;
        }

        // ------------------- Cập nhật Customer -------------------
        public async Task<bool> UpdateCustomerAsync(UpdateCustomerDto dto)
        {
            var customer = await _context.Customers.Include(c => c.Account).FirstOrDefaultAsync(c => c.CustomerId == dto.CustomerId);
            if (customer == null) throw new KeyNotFoundException("Không tìm thấy khách hàng.");

            customer.FullName = dto.FullName ?? customer.FullName;
            customer.Phone = dto.Phone ?? customer.Phone;
            customer.Address = dto.Address ?? customer.Address;
            customer.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        // ------------------- Cập nhật Company -------------------
        public async Task<bool> UpdateCompanyAsync(UpdateCompanyDto dto)
        {
            var company = await _context.Companies.FindAsync(dto.CompanyId);
            if (company == null) throw new KeyNotFoundException("Không tìm thấy công ty.");

            company.Name = dto.Name ?? company.Name;
            company.TaxCode = dto.TaxCode ?? company.TaxCode;
            company.Email = dto.Email ?? company.Email;
            company.Phone = dto.Phone ?? company.Phone;
            company.Address = dto.Address ?? company.Address;
            company.ImageUrl = dto.ImageUrl ?? company.ImageUrl;
            company.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        // ------------------- Upload avatar S3 -------------------
        public async Task<string> UpdateAvatarAsync(int accountId, IFormFile file)
        {
            var account = await _accountRepository.GetByIdAsync(accountId);
            if (account == null) throw new KeyNotFoundException("Không tìm thấy tài khoản.");

            if (!string.IsNullOrEmpty(account.AvatarUrl))
                await _s3Service.DeleteFileAsync(account.AvatarUrl);

            var imageUrl = await _s3Service.UploadFileAsync(file, "avatars");
            account.AvatarUrl = imageUrl;
            account.UpdatedAt = DateTime.Now;
            await _accountRepository.UpdateAsync(account);

            return imageUrl;
        }
    }
}
