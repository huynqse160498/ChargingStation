using Microsoft.AspNetCore.Mvc;
using Repositories.DTOs.PricingRules;
using Services.Interfaces;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PricingRuleController : ControllerBase
    {
        private readonly IPricingRuleService _service;

        public PricingRuleController(IPricingRuleService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PricingRuleQueryDto query)
        {
            var result = await _service.GetAllAsync(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var rule = await _service.GetByIdAsync(id);
            if (rule == null)
                return NotFound(new { message = "Không tìm thấy quy tắc giá." });

            return Ok(rule);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PricingRuleCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var msg = await _service.CreateAsync(dto);
            if (msg.Contains("Không") || msg.Contains("Đã tồn tại"))
                return BadRequest(new { message = msg });

            return Ok(new { message = msg });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] PricingRuleUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var msg = await _service.UpdateAsync(id, dto);
            if (msg.Contains("Không"))
                return BadRequest(new { message = msg });

            return Ok(new { message = msg });
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> ChangeStatus(int id, [FromBody] PricingRuleChangeStatusDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var msg = await _service.ChangeStatusAsync(id, dto.Status);
            if (msg.Contains("Không"))
                return BadRequest(new { message = msg });

            return Ok(new { message = msg });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var msg = await _service.DeleteAsync(id);
            if (msg.Contains("Không"))
                return NotFound(new { message = msg });

            return Ok(new { message = msg });
        }
    }
}
