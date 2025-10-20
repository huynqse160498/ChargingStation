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

        // ======================= [BASIC CRUD] =======================

        // GET: /api/chargers
        [HttpGet]
        public async Task<IActionResult> GetAll()
            => Ok(await _service.GetAllAsync());

        // GET: /api/chargers/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            try { return Ok(await _service.GetByIdAsync(id)); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        // POST: /api/chargers  (mặc định Status = "Online")
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

        // PUT: /api/chargers/{id}
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

        // DELETE: /api/chargers/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _service.DeleteAsync(id);
            return ok ? NoContent() : NotFound();
        }

        // ======================= [PAGING + FILTER] =======================

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

        // ======================= [CHANGE STATUS] =======================

        public class ChargerChangeStatusRequest { public string Status { get; set; } = string.Empty; }

        // PATCH: /api/chargers/{id}/status
        [HttpPatch("{id:int}/status")]
        public async Task<IActionResult> ChangeStatus(int id, [FromBody] ChargerChangeStatusRequest req)
        {
            if (string.IsNullOrWhiteSpace(req?.Status))
                return BadRequest(new { message = "Status không được trống." });

            var value = req.Status.Trim();
            if (value != "Online" && value != "Offline" && value != "OutOfOrder")
                return BadRequest(new { message = "Status chỉ nhận 'Online', 'Offline' hoặc 'OutOfOrder'." });

            var ok = await _service.ChangeStatusAsync(id, value);
            return ok ? NoContent() : NotFound();
        }

        // ======================= [UPLOAD IMAGE] =======================
        // POST: /api/chargers/image/upload
        [HttpPost("image/upload")]
        [Consumes("multipart/form-data")] // NEW
        [ProducesResponseType(typeof(ChargerReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UploadImage([FromForm] ChargerImageUploadDto form) // NEW
        {
            try
            {
                var dto = await _service.UploadImageAsync(form.ChargerId, form.File);
                return Ok(dto);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        }
    }
}
