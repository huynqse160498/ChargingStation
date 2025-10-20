using Microsoft.AspNetCore.Mvc;
using Repositories.DTOs.SubscriptionPlans;
using Services.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace ChargingStationSystem.Controllers
{
    // NEW
    [ApiController]
    [Route("api/[controller]")]
    public class SubscriptionPlansController : ControllerBase
    {
        private readonly ISubscriptionPlanService _svc;
        public SubscriptionPlansController(ISubscriptionPlanService svc) { _svc = svc; } 

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _svc.GetAllAsync());       

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            try { return Ok(await _svc.GetByIdAsync(id)); }                             
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SubscriptionPlanCreateDto input)
        {
            try
            {
                var created = await _svc.CreateAsync(input);                             
                return CreatedAtAction(nameof(GetById), new { id = created.SubscriptionPlanId }, created);
            }
            catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] SubscriptionPlanUpdateDto input)
        {
            try { return Ok(await _svc.UpdateAsync(id, input)); }                        
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try { await _svc.DeleteAsync(id); return NoContent(); }                      
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }
    }
}