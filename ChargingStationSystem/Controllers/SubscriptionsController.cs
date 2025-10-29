using Microsoft.AspNetCore.Mvc;
using Repositories.DTOs.Subscriptions;
using Services.Interfaces;

namespace ChargingStationSystem.Controllers
{
    // ======================= [ CONTROLLER - SUBSCRIPTION ] =======================
    [ApiController]
    [Route("api/[controller]")]
    public class SubscriptionsController : ControllerBase
    {
        private readonly ISubscriptionService _svc;
        public SubscriptionsController(ISubscriptionService svc) => _svc = svc;

        // ======================= [ GET - ALL / SINGLE ] =======================
        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _svc.GetAllAsync());

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            try { return Ok(await _svc.GetByIdAsync(id)); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
        }

        // ======================= [ CREATE / UPDATE / DELETE ] ===================
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SubscriptionCreateDto input)
        {
            try
            {
                var created = await _svc.CreateAsync(input);
                return CreatedAtAction(nameof(GetById), new { id = created.SubscriptionId }, created);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] SubscriptionUpdateDto input)
        {
            try { return Ok(await _svc.UpdateAsync(id, input)); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try { await _svc.DeleteAsync(id); return NoContent(); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }
    }
}
