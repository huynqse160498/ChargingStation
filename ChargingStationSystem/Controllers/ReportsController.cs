using Microsoft.AspNetCore.Mvc;
using Repositories.DTOs.Reports;
using Services.Interfaces;

namespace ChargingStationSystem.Controllers
{
    [ApiController]
    [Route("api/reports")]
    // [Authorize] // bật nếu cần
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _svc;
        public ReportsController(IReportService svc) => _svc = svc;

        // GET: /api/reports?stationId=&chargerId=&status=&severity=&from=&to=&page=1&pageSize=10
        [HttpGet]
        public async Task<IActionResult> GetPaged(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? stationId = null,
            [FromQuery] int? chargerId = null,
            [FromQuery] string? status = null,
            [FromQuery] string? severity = null,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null)
        {
            var (total, items) = await _svc.GetPagedAsync(page, pageSize, stationId, chargerId, status, severity, from, to);
            return Ok(new { total, items });
        }

        // GET: /api/reports/123
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            try { return Ok(await _svc.GetAsync(id)); }
            catch (KeyNotFoundException) { return NotFound("Report không tồn tại."); }
        }

        // POST: /api/reports
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ReportCreateDto dto)
        {
            var created = await _svc.CreateAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = created.ReportId }, created);
        }

        // PUT: /api/reports/123
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] ReportUpdateDto dto)
        {
            try { return Ok(await _svc.UpdateAsync(id, dto)); }
            catch (KeyNotFoundException) { return NotFound("Report không tồn tại."); }
        }

        // DELETE: /api/reports/123
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _svc.DeleteAsync(id);
            return ok ? NoContent() : NotFound();
        }
    }
}