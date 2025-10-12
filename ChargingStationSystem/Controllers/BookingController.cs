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

    
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] BookingDtos.Query query)
        {
            var result = await _service.GetAllAsync(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var booking = await _service.GetByIdAsync(id);
            if (booking == null)
                return NotFound(new { message = "Không tìm thấy đặt lịch." });

            return Ok(booking);
        }

     
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] BookingDtos.Create dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var message = await _service.CreateAsync(dto);
            if (message.Contains("không") || message.Contains("trước"))
                return BadRequest(new { message });

            return Ok(new { message });
        }

  
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

 
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var message = await _service.DeleteAsync(id);
            if (message.Contains("Không tìm"))
                return NotFound(new { message });

            return Ok(new { message });
        }
    }
}
