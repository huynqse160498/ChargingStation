using Microsoft.AspNetCore.Mvc;
using Repositories.DTOs.StationStaff;
using Services.Implementations;
using Services.Interfaces;

namespace ChargingStationSystem.Controllers
{
    [ApiController]
    [Route("api/station-staffs")]
    // [Authorize] // bật nếu cần auth
    public class StationStaffsController : ControllerBase
    {
        private readonly IStationStaffService _service;
        public StationStaffsController(IStationStaffService service) => _service = service;


        [HttpGet]
        public async Task<IActionResult> List([FromQuery] int stationId)
        {
            try
            {
                var data = await _service.ListByStationAsync(stationId);
                return Ok(data);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Station không tồn tại.");
            }
        }


        [HttpPost]
        public async Task<IActionResult> Create([FromBody] StationStaffCreateDto dto)
        {
            var created = await _service.AddAsync(dto);
            // Trả về link list theo station
            return Created($"/api/station-staffs?stationId={created.StationId}", created);
        }

       
        [HttpDelete("{stationId:int}/{staffId:int}")]
        public async Task<IActionResult> Delete(int stationId, int staffId)
        {
            var ok = await _service.DeleteAsync(stationId, staffId);
            return ok ? NoContent() : NotFound();
        }
    }
}

