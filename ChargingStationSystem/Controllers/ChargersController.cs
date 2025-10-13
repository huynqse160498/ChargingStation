using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories.DTOs.Chargers;
using Services.Interfaces;

namespace ChargingStationSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChargersController : ControllerBase
    {
        private readonly IChargerService _service;
        public ChargersController(IChargerService service) { _service = service; }

        [HttpGet]
        public async Task<IActionResult> GetAll()
            => Ok(await _service.GetAllAsync());

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            try { return Ok(await _service.GetByIdAsync(id)); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ChargerCreateDto dto)
        {
            try
            {
                var created = await _service.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.ChargerId }, created);
            }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] ChargerUpdateDto dto)
        {
            try
            {
                var ok = await _service.UpdateAsync(id, dto);
                return ok ? NoContent() : NotFound();
            }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _service.DeleteAsync(id);
            return ok ? NoContent() : NotFound();
        }

        // GET: /api/chargers/paged?stationId=&code=&type=&status=&minPower=&maxPower=&page=1&pageSize=20
        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] int? stationId = null,
            [FromQuery] string? code = null,
            [FromQuery] string? type = null,
            [FromQuery] string? status = null,
            [FromQuery] decimal? minPower = null,
            [FromQuery] decimal? maxPower = null)
        {
            var (items, total) = await _service.GetPagedAsync(page, pageSize, stationId, code, type, status, minPower, maxPower);
            return Ok(new { page, pageSize, total, items });
        }

        // PATCH: /api/chargers/{id}/status
        public class ChargerChangeStatusRequest { public string Status { get; set; } = string.Empty; }

        [HttpPatch("{id:int}/status")]
        public async Task<IActionResult> ChangeStatus(int id, [FromBody] ChargerChangeStatusRequest req)
        {
            if (string.IsNullOrWhiteSpace(req?.Status))
                return BadRequest(new { message = "Status không được trống." });

            var ok = await _service.ChangeStatusAsync(id, req.Status);
            return ok ? NoContent() : NotFound();
        }
    }
}   