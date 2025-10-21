using Microsoft.AspNetCore.Mvc;
using Repositories.DTOs;
using Services.Interfaces;

namespace ChargingStationSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChargingSessionsController : ControllerBase
    {
        private readonly IChargingSessionService _service;

        public ChargingSessionsController(IChargingSessionService service)
        {
            _service = service;
        }

        // 🔹 Bắt đầu phiên sạc
        [HttpPost("start")]
        public async Task<IActionResult> Start([FromBody] ChargingSessionCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var session = await _service.StartSessionAsync(dto);

                string msg = dto.BookingId != null
                    ? "✅ Bắt đầu phiên sạc theo Booking thành công!"
                    : "⚡ Bắt đầu phiên sạc trực tiếp thành công!";

                return Ok(new
                {
                    message = msg,
                    data = new
                    {
                        session.ChargingSessionId,
                        session.PortId,
                        session.VehicleId,
                        session.CustomerId,
                        session.Status,
                        session.StartedAt,
                        PricingRule = session.PricingRuleId
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

        // 🔹 Kết thúc phiên sạc
        [HttpPost("end")]
        public async Task<IActionResult> End([FromBody] ChargingSessionEndDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var session = await _service.EndSessionAsync(dto);
                return Ok(new
                {
                    message = "✅ Phiên sạc đã kết thúc thành công!",
                    data = new
                    {
                        session.ChargingSessionId,
                        session.Total,
                        session.Subtotal,
                        session.Tax,
                        session.EndSoc,
                        session.EndedAt,
                        session.Status
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

        // 🔹 Lấy toàn bộ phiên sạc
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var sessions = await _service.GetAllAsync();
            return Ok(sessions);
        }

        // 🔹 Lấy chi tiết 1 phiên sạc
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var session = await _service.GetByIdAsync(id);
            if (session == null)
                return NotFound(new { message = "Không tìm thấy phiên sạc." });

            return Ok(session);
        }

        // 🔹 Xóa phiên sạc
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _service.DeleteAsync(id);
                return Ok(new { message = "🗑️ Đã xóa phiên sạc thành công." });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
        }
    }
}
