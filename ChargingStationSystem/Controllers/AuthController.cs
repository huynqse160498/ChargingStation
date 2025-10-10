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
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var result = await _authService.RegisterAsync(dto);
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var accounts = await _authService.GetAllAsync();
            return Ok(accounts);
        }

        // ------------------- Lấy tài khoản theo ID -------------------
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetById(int id)
        {
            var account = await _authService.GetByIdAsync(id);
            return Ok(account);
        }

        // ------------------- Xóa tài khoản -------------------
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _authService.DeleteAsync(id);
            if (!success) return NotFound(new { message = "Không tìm thấy tài khoản" });
            return Ok(new { message = "Xóa tài khoản thành công" });
        }

        // ------------------- Đổi vai trò (chỉ admin) -------------------
        [HttpPut("changerole/{accountId}")]
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin,Staff")]
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
    }
}
