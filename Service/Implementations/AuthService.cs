using Repositories.DTOs;
using Repositories.Interfaces;
using Repositories.Models;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ChargeStationContext _context;
        private readonly IPasswordHasher<Account> _passwordHasher;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        public AuthService(
            IAccountRepository accountRepository,
            ChargeStationContext context,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration)
        {
            _accountRepository = accountRepository;
            _context = context;
            _passwordHasher = new PasswordHasher<Account>();
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        // ------------------- Đăng ký -------------------
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

            // Mã hóa mật khẩu
            account.PassWord = _passwordHasher.HashPassword(account, dto.Password);

            // Lưu tài khoản
            await _accountRepository.AddAsync(account);

            // 🔗 Tạo Customer tương ứng (1-1)
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

            // 🔐 Tạo JWT Token
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

            string tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return new
            {
                Success = true,
                Message = "Đăng nhập thành công",
                Token = tokenString
            };
        }

        // ------------------- Lấy tất cả tài khoản -------------------
        public async Task<IEnumerable<Account>> GetAllAsync()
        {
            return await _accountRepository.GetAllAsync();
        }

        // ------------------- Lấy tài khoản theo ID -------------------
        public async Task<Account> GetByIdAsync(int id)
        {
            var account = await _accountRepository.GetByIdAsync(id);
            if (account == null)
                throw new KeyNotFoundException("Không tìm thấy tài khoản.");
            return account;
        }

        // ------------------- Xóa tài khoản -------------------
        public async Task<bool> DeleteAsync(int id)
        {
            var account = await _accountRepository.GetByIdAsync(id);
            if (account == null)
                throw new KeyNotFoundException("Không tìm thấy tài khoản.");

            await _accountRepository.DeleteAsync(account);
            return true;
        }

        // ------------------- Đổi vai trò (chỉ Admin) -------------------
        public async Task<bool> ChangeRoleAsync(int accountId, string newRole)
        {
            var currentUser = _httpContextAccessor.HttpContext?.User;
            if (currentUser == null)
                throw new UnauthorizedAccessException("Người dùng chưa đăng nhập.");

            var currentRole = currentUser.FindFirst(ClaimTypes.Role)?.Value;
            if (currentRole == null || !currentRole.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException("Chỉ Admin mới có quyền đổi vai trò.");

            var account = await _accountRepository.GetByIdAsync(accountId);
            if (account == null)
                throw new KeyNotFoundException("Không tìm thấy tài khoản.");

            account.Role = newRole;
            await _accountRepository.UpdateAsync(account);
            return true;
        }

        // ------------------- Đổi trạng thái tài khoản -------------------
        public async Task<bool> ChangeStatusAsync(int accountId, string newStatus)
        {
            var account = await _accountRepository.GetByIdAsync(accountId);
            if (account == null)
                throw new KeyNotFoundException("Không tìm thấy tài khoản.");

            account.Status = newStatus;
            await _accountRepository.UpdateAsync(account);
            return true;
        }
    }
}
