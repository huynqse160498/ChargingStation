using Microsoft.AspNetCore.Mvc;
using Repositories.DTOs;
using Services.Interfaces;
using System.Threading.Tasks;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentCrudController : ControllerBase
    {
        private readonly IPaymentCrudService _paymentCrudService;

        public PaymentCrudController(IPaymentCrudService paymentCrudService)
        {
            _paymentCrudService = paymentCrudService;
        }

        // =======================================================
        // 🔹 GET: api/paymentcrud
        // Lấy toàn bộ danh sách thanh toán
        // =======================================================
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var payments = await _paymentCrudService.GetAllAsync();
            return Ok(new
            {
                success = true,
                total = payments?.Count() ?? 0,
                data = payments
            });
        }

        // =======================================================
        // 🔹 GET: api/paymentcrud/{id}
        // Lấy chi tiết 1 payment
        // =======================================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var payment = await _paymentCrudService.GetByIdAsync(id);
            if (payment == null)
                return NotFound(new { success = false, message = "Không tìm thấy thanh toán." });

            return Ok(new { success = true, data = payment });
        }

        // =======================================================
        // 🔹 PUT: api/paymentcrud/{id}
        // Cập nhật thông tin thanh toán
        // =======================================================
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] PaymentUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _paymentCrudService.UpdateAsync(id, dto);
            if (!result)
                return NotFound(new { success = false, message = "Không tìm thấy thanh toán để cập nhật." });

            return Ok(new { success = true, message = "Cập nhật thanh toán thành công." });
        }

        // =======================================================
        // 🔹 DELETE: api/paymentcrud/{id}
        // Xóa thanh toán
        // =======================================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _paymentCrudService.DeleteAsync(id);
            if (!result)
                return NotFound(new { success = false, message = "Không tìm thấy thanh toán để xóa." });

            return Ok(new { success = true, message = "Xóa thanh toán thành công." });
        }
        [HttpGet("by-session/{sessionId:int}")]
        public async Task<IActionResult> GetBySession(int sessionId)
        {
            var payments = await _paymentCrudService.GetByChargingSessionAsync(sessionId);
            return Ok(new { success = true, total = payments?.Count() ?? 0, data = payments });
        }

    }
}
