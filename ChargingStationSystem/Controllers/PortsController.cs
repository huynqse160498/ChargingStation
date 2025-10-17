using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories.DTOs.Ports;
using Services.Implementations;
using Services.Interfaces;

namespace ChargingStationSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PortsController : ControllerBase
    {
        private readonly IPortService _service;
        private readonly IS3Service _s3Service; // NEW s3Service


        public PortsController(IPortService service, IS3Service s3Service)
        {
            _service = service;
            _s3Service = s3Service; // NEW
        }


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

        // ======================= [UPLOAD IMAGE TO S3] =======================
        // POST: api/ports/upload
        // Form-data: file = (chọn ảnh)
        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "File rỗng hoặc không hợp lệ." });

            try
            {
                // upload vào thư mục "ports" trong bucket
                var imageUrl = await _s3Service.UploadFileAsync(file, "ports");
                return Ok(new { imageUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi upload file", error = ex.Message });
            }
        }
    }
}