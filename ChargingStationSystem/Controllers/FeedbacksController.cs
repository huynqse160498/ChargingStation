
using Microsoft.AspNetCore.Mvc;
using Repositories.DTOs.Feedbacks;
using Services.Interfaces;

namespace ChargingStationSystem.Controllers
{
    [ApiController]
    [Route("api/feedbacks")]
    // [Authorize]
    public class FeedbacksController : ControllerBase
    {
        private readonly IFeedbackService _svc;
        public FeedbacksController(IFeedbackService svc) => _svc = svc;

        // GET: /api/feedbacks?stationId=1&page=1&pageSize=10&customerId=&rating=
        [HttpGet]
        public async Task<IActionResult> GetPaged(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? stationId = null,
            [FromQuery] int? customerId = null,
            [FromQuery] int? rating = null)
        {
            var (total, items) = await _svc.GetPagedAsync(page, pageSize, stationId, customerId, rating);
            return Ok(new { total, items });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
            => Ok(await _svc.GetAsync(id));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] FeedbackCreateDto dto)
        {
            var created = await _svc.CreateAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = created.FeedbackId }, created);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] FeedbackUpdateDto dto)
            => Ok(await _svc.UpdateAsync(id, dto));

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
            => (await _svc.DeleteAsync(id)) ? NoContent() : NotFound();
    }
}
