using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories.DTOs.Stations;
using Services.Interfaces;

namespace ChargingStationSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StationsController : ControllerBase
    {
        private readonly IStationService _service;
        public StationsController(IStationService service)
        {
            _service = service;
        }


        // GET: /api/stations

        // ======================= [BASIC CRUD] =======================
        [HttpGet]
        public async Task<IActionResult> GetAll()
            => Ok(await _service.GetAllAsync());


        // GET: /api/stations/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            try { return Ok(await _service.GetByIdAsync(id)); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }


        // POST: /api/stations  (mặc định Status = "Open")
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] StationCreateDto dto)
        {
            try
            {
                var created = await _service.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.StationId }, created);
            }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        }


        // PUT: /api/stations/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] StationUpdateDto dto)
        {
            try
            {
                var ok = await _service.UpdateAsync(id, dto);
                return ok ? NoContent() : NotFound();
            }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        }


        // DELETE: /api/stations/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _service.DeleteAsync(id);
            return ok ? NoContent() : NotFound();
        }

        // ======================= [PAGING + FILTER] =======================
        // GET: /api/stations/paged?stationName=&city=&status=&page=1&pageSize=20
        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? stationName = null,
            [FromQuery] string? city = null,
            [FromQuery] string? status = null)
        {
            var (items, total) = await _service.GetPagedAsync(page, pageSize, stationName, city, status);
            return Ok(new { page, pageSize, total, items });
        }


        // ======================= [CHANGE STATUS] =======================
        // PATCH: /api/stations/{id}/status
        public class StationChangeStatusRequest { public string Status { get; set; } = string.Empty; }

        [HttpPatch("{id:int}/status")]
        public async Task<IActionResult> ChangeStatus(int id, [FromBody] StationChangeStatusRequest req)
        {
            if (string.IsNullOrWhiteSpace(req?.Status))
                return BadRequest(new { message = "Status không được trống." });

            var value = req.Status.Trim();
            if (value != "Open" && value != "Closed")
                return BadRequest(new { message = "Status chỉ nhận 'Open' hoặc 'Closed'." });

            var ok = await _service.ChangeStatusAsync(id, value);
            return ok ? NoContent() : NotFound();
        }
    }
}