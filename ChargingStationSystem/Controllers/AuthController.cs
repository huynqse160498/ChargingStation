using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories.DTOs;
using Repositories.Models;
using Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace ChargingStationSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // ------------------- Đăng ký -------------------
        // Đăng ký KH cá nhân (giữ như hiện tại)
        [HttpPost("register-customer")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterCustomer([FromBody] RegisterDto dto)
        {
            var result = await _authService.RegisterAsync(dto);
            return Ok(new { message = result });
        }

        // ⬇️ Thêm mới: Đăng ký doanh nghiệp
        [HttpPost("register-company")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterCompany([FromBody] RegisterCompanyDto dto)
        {
            var result = await _authService.RegisterCompanyAsync(dto);
            return Ok(new { message = result });
        }

        // ------------------- Đăng nhập -------------------
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var result = await _authService.LoginAsync(dto);
            return Ok(new { message = result });
        }

        // ------------------- Lấy danh sách tài khoản -------------------
        [HttpGet]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var accounts = await _authService.GetAllAsync();
            return Ok(accounts);
        }

        // ------------------- Lấy tài khoản theo ID -------------------
        [HttpGet("{id}")]
        //[Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetById(int id)
        {
            var account = await _authService.GetByIdAsync(id);
            return Ok(account);
        }

        // ------------------- Xóa tài khoản -------------------
        [HttpDelete("{id}")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _authService.DeleteAsync(id);
            if (!success) return NotFound(new { message = "Không tìm thấy tài khoản" });
            return Ok(new { message = "Xóa tài khoản thành công" });
        }

        // ------------------- Đổi vai trò (chỉ admin) -------------------
        [HttpPut("changerole/{accountId}")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> ChangeRole(int accountId, [FromQuery] string newRole)
        {
            try
            {
                var success = await _authService.ChangeRoleAsync(accountId, newRole);
                if (!success) return BadRequest(new { message = "Đổi vai trò thất bại" });
                return Ok(new { message = "Đổi vai trò thành công" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // ------------------- Đổi trạng thái tài khoản -------------------
        [HttpPut("changestatus/{accountId}")]
        //[Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> ChangeStatus(int accountId, [FromQuery] string newStatus)
        {
            try
            {
                var success = await _authService.ChangeStatusAsync(accountId, newStatus);
                if (!success) return BadRequest(new { message = "Đổi trạng thái thất bại" });
                return Ok(new { message = "Cập nhật trạng thái thành công" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
        [HttpPut("update-customer")]
        public async Task<IActionResult> UpdateCustomer([FromBody] UpdateCustomerDto dto)
             => Ok(new { message = await _authService.UpdateCustomerAsync(dto) ? "Cập nhật khách hàng thành công" : "Thất bại" });

        [HttpPut("update-company")]
        public async Task<IActionResult> UpdateCompany([FromBody] UpdateCompanyDto dto)
            => Ok(new { message = await _authService.UpdateCompanyAsync(dto) ? "Cập nhật công ty thành công" : "Thất bại" });

        [HttpPost("upload-avatar/{accountId}")]
        public async Task<IActionResult> UploadAvatar(int accountId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Vui lòng chọn ảnh để tải lên" });

            var imageUrl = await _authService.UpdateAvatarAsync(accountId, file);
            return Ok(new { message = "Tải ảnh đại diện thành công", avatarUrl = imageUrl });
        }
        // -------------------- 🔹 Đổi mật khẩu --------------------
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var message = await _authService.ChangePasswordAsync(dto);
            return Ok(new { message });
        }

        // -------------------- 🔹 Quên mật khẩu --------------------
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            var message = await _authService.ForgotPasswordAsync(dto);
            return Ok(new { message });
        }

        // -------------------- 🔹 Đặt lại mật khẩu --------------------
        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var message = await _authService.ResetPasswordAsync(dto);
            return Ok(new { message });
        }
        [HttpPost("login-google")]
        [AllowAnonymous]
        public async Task<IActionResult> LoginGoogle([FromBody] GoogleLoginDto dto)
        {
            var result = await _authService.GoogleLoginAsync(dto);
            return Ok(result);
        }

    }
}
