using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories.DTOs.Ports;
using Services.Interfaces;

namespace ChargingStationSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PortsController : ControllerBase
    {
        private readonly IPortService _service;
        public PortsController(IPortService service) { _service = service; }


        // ======================= [BASIC CRUD] =======================

        // GET: api/ports
        // Lấy toàn bộ danh sách port (ít dùng, chủ yếu cho test hoặc admin)

        [HttpGet]
        public async Task<IActionResult> GetAll()
            => Ok(await _service.GetAllAsync());
        // GET: api/ports/{id}
        // Lấy port theo Id


        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                return Ok(await _service.GetByIdAsync(id));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // POST: api/ports
        // Tạo port mới – mặc định Status = "Available"
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PortCreateDto dto)
        {
            try
            {
                var created = await _service.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.PortId }, created);
            }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        }

        // PUT: api/ports/{id}
        // Cập nhật thông tin port (giữ nguyên status hoặc cập nhật nếu gửi vào)
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] PortUpdateDto dto)
        {
            try
            {
                var ok = await _service.UpdateAsync(id, dto);
                return ok ? NoContent() : NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE: api/ports/{id}
        // Xóa port theo Id
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _service.DeleteAsync(id);
            return ok ? NoContent() : NotFound();
        }

        // ======================= [PAGING + FILTER] =======================

        // GET: api/ports/paged?chargerId=&status=&page=1&pageSize=20
        // Lấy danh sách port có phân trang và filter theo charger/status
        [HttpGet("paged")]

        public async Task<IActionResult> GetPaged(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] int? chargerId = null,
            [FromQuery] string? status = null)
        {
            var (items, total) = await _service.GetPagedAsync(page, pageSize, chargerId, status);
            return Ok(new
            {
                page,
                pageSize,
                total,
                items
            });
        }

        // ======================= [CHANGE STATUS] =======================

        // Dùng khi staff muốn đổi trạng thái port:
        // PATCH: api/ports/{id}/status
        // Body: { "status": "Reserved" }
        public class ChangeStatusRequest { public string Status { get; set; } = string.Empty; }

        [HttpPatch("{id:int}/status")]
        public async Task<IActionResult> ChangeStatus(int id, [FromBody] ChangeStatusRequest req)
        {
            var ok = await _service.ChangeStatusAsync(id, req.Status);
            return ok ? NoContent() : NotFound();
        }
        // ======================= [UPLOAD IMAGE] =======================
        // POST: /api/ports/image/upload
        [HttpPost("image/upload")]
        [Consumes("multipart/form-data")] // NEW
        [ProducesResponseType(typeof(PortReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UploadImage([FromForm] PortImageUploadDto form)
        {
            try
            {
                var dto = await _service.UploadImageAsync(form.PortId, form.File);
                return Ok(dto);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        }
    }
}
    
