using Microsoft.AspNetCore.Mvc;
using Repositories.DTOs;
using Services.Interfaces;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _service;

        public BookingController(IBookingService service)
        {
            _service = service;
        }

        // =============================
        // 🔹 Lấy danh sách (có phân trang)
        // =============================
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] BookingDtos.Query query)
        {
            var result = await _service.GetAllAsync(query);
            return Ok(result);
        }

        // =============================
        // 🔹 Lấy chi tiết 1 booking
        // =============================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var booking = await _service.GetByIdAsync(id);
            if (booking == null)
                return NotFound(new { message = "Không tìm thấy đặt lịch." });

            return Ok(booking);
        }

        // =============================
        // 🔹 Tạo mới Booking (Customer hoặc Company)
        // =============================
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] BookingDtos.Create dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // ✅ Cho phép booking của Customer hoặc Company
            if (dto.CustomerId == 0 && dto.CompanyId == null)
                return BadRequest(new { message = "Cần cung cấp CustomerId hoặc CompanyId để đặt lịch." });

            // ✅ Set mặc định trạng thái = Pending
            dto.Status ??= "Pending";

            var message = await _service.CreateAsync(dto);

            if (message.Contains("không") || message.Contains("trước"))
                return BadRequest(new { message });

            return Ok(new { message });
        }

        // =============================
        // 🔹 Cập nhật Booking
        // =============================
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] BookingDtos.Update dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var message = await _service.UpdateAsync(id, dto);
            if (message.Contains("không") || message.Contains("trước"))
                return BadRequest(new { message });

            return Ok(new { message });
        }

        // =============================
        // 🔹 Xóa Booking
        // =============================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var message = await _service.DeleteAsync(id);
            if (message.Contains("Không tìm"))
                return NotFound(new { message });

            return Ok(new { message });
        }

        // =============================
        // 🔹 Đổi trạng thái Booking
        // =============================
        [HttpPut("{id}/status")]
        public async Task<IActionResult> ChangeStatus(int id, [FromBody] BookingDtos.ChangeStatus dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var message = await _service.ChangeStatusAsync(id, dto.Status);
            if (message.Contains("không"))
                return BadRequest(new { message });

            return Ok(new { message });
        }
    }
}
