using Microsoft.AspNetCore.Mvc;
using Repositories.DTOs;
using Services.Interfaces;

namespace ChargingStationSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoicesController : ControllerBase
    {
        private readonly IInvoiceService _service;

        public InvoicesController(IInvoiceService service)
        {
            _service = service;
        }

        // 🔹 [ADMIN] Lấy tất cả hóa đơn
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var invoices = await _service.GetAllAsync();
            return Ok(new
            {
                message = "✅ Danh sách hóa đơn",
                total = invoices.Count,
                data = invoices
            });
        }

        // 🔹 [CUSTOMER] Xem hóa đơn của chính mình
        [HttpGet("by-customer/{customerId}")]
        public async Task<IActionResult> GetByCustomer(int customerId)
        {
            var invoices = await _service.GetByCustomerIdAsync(customerId);
            if (!invoices.Any())
                return NotFound(new { message = "Khách hàng chưa có hóa đơn nào." });

            return Ok(new
            {
                message = $"✅ {invoices.Count} hóa đơn của khách hàng #{customerId}",
                data = invoices
            });
        }

        // 🔹 [COMPANY] Xem tất cả hóa đơn của nhân viên trong công ty
        [HttpGet("by-company/{companyId}")]
        public async Task<IActionResult> GetByCompany(int companyId)
        {
            var invoices = await _service.GetByCompanyIdAsync(companyId);
            if (!invoices.Any())
                return NotFound(new { message = "Công ty này chưa có hóa đơn nào." });

            return Ok(new
            {
                message = $"✅ {invoices.Count} hóa đơn của công ty #{companyId}",
                data = invoices
            });
        }

        // 🔹 Xem chi tiết hóa đơn
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var invoice = await _service.GetByIdAsync(id);
            if (invoice == null)
                return NotFound(new { message = "❌ Không tìm thấy hóa đơn." });

            return Ok(new
            {
                message = "✅ Chi tiết hóa đơn",
                data = invoice
            });
        }

        // 🔹 [ADMIN] Tạo hóa đơn thủ công (ngoài hóa đơn tự sinh)
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] InvoiceCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var invoice = await _service.CreateAsync(dto);
                return Ok(new
                {
                    message = "✅ Tạo hóa đơn thủ công thành công!",
                    data = invoice
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.InnerException?.Message ?? ex.Message });
            }
        }

        // 🔹 [ADMIN] Cập nhật trạng thái (Paid / Overdue / Unpaid)
        [HttpPut("status")]
        public async Task<IActionResult> UpdateStatus([FromBody] InvoiceUpdateStatusDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _service.UpdateStatusAsync(dto);
                return Ok(new { message = $"✅ Cập nhật trạng thái hóa đơn #{dto.InvoiceId} thành công ({dto.Status})." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.InnerException?.Message ?? ex.Message });
            }
        }

        // 🔹 [ADMIN] Xóa hóa đơn
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _service.DeleteAsync(id);
                return Ok(new { message = "🗑️ Đã xóa hóa đơn thành công." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.InnerException?.Message ?? ex.Message });
            }
        }
    }
}
