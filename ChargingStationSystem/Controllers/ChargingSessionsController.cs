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

        [HttpPost("start")]
        public async Task<IActionResult> Start([FromBody] ChargingSessionCreateDto dto)
        {
            try
            {
                var session = await _service.StartSessionAsync(dto);
                return Ok(new
                {
                    message = dto.BookingId != null
                        ? "Bắt đầu sạc theo booking thành công!"
                        : "Bắt đầu sạc trực tiếp thành công!",
                    data = session
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("end")]
        public async Task<IActionResult> End([FromBody] ChargingSessionEndDto dto)
        {
            try
            {
                var session = await _service.EndSessionAsync(dto);
                return Ok(new { message = "Đã kết thúc phiên sạc!", data = session });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var session = await _service.GetByIdAsync(id);
            if (session == null) return NotFound();
            return Ok(session);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _service.DeleteAsync(id);
                return Ok(new { message = "Đã xóa phiên sạc" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
