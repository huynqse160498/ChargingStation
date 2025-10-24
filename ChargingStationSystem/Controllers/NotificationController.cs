using Microsoft.AspNetCore.Mvc;
using Repositories.DTOs.Notifications;
using Services.Interfaces;
using System.Threading.Tasks;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _service;

        public NotificationController(INotificationService service)
        {
            _service = service;
        }

        // ===== basic CRUD / queries =====
        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _service.GetAllAsync());

        [HttpGet("customer/{id}")]
        public async Task<IActionResult> GetByCustomer(int id, [FromQuery] bool includeArchived = false) =>
            Ok(await _service.GetByCustomerAsync(id, includeArchived));

        [HttpGet("company/{id}")]
        public async Task<IActionResult> GetByCompany(int id, [FromQuery] bool includeArchived = false) =>
            Ok(await _service.GetByCompanyAsync(id, includeArchived));

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id) =>
            Ok(await _service.GetByIdAsync(id));

        [HttpPost] // system/manual create
        public async Task<IActionResult> Create([FromBody] NotificationCreateDto dto) =>
            Ok(await _service.CreateAsync(dto));

        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            await _service.MarkAsReadAsync(id);
            return Ok(new { success = true });
        }

        [HttpPut("{id}/archive")]
        public async Task<IActionResult> Archive(int id)
        {
            await _service.ArchiveAsync(id);
            return Ok(new { success = true });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return Ok(new { success = true });
        }

        // ===== ADMIN SEND =====
        [HttpPost("admin/send-to-customer")]
        public async Task<IActionResult> AdminSendToCustomer([FromBody] AdminSendToCustomerDto dto)
            => Ok(await _service.AdminSendToCustomerAsync(dto));

        [HttpPost("admin/send-to-company")]
        public async Task<IActionResult> AdminSendToCompany([FromBody] AdminSendToCompanyDto dto)
            => Ok(await _service.AdminSendToCompanyAsync(dto));

        [HttpPost("admin/broadcast")]
        public async Task<IActionResult> AdminBroadcast([FromBody] AdminBroadcastDto dto)
        {
            var count = await _service.AdminBroadcastAsync(dto);
            return Ok(new { success = true, sent = count });
        }
    }
}
