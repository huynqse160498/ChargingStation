using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories.DTOs.Vehicles;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChargingStationSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VehiclesController : ControllerBase
    {
        private readonly IVehicleService _svc;
        public VehiclesController(IVehicleService svc) => _svc = svc;

        // ======================= [GET - PAGED] =======================
        // GET: /api/vehicles?page=1&pageSize=10&status=Active&carMaker=Tesla...
        [HttpGet]
        public async Task<IActionResult> GetPaged(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? licensePlate = null,
            [FromQuery] string? carMaker = null,
            [FromQuery] string? model = null,
            [FromQuery] string? status = null,
            [FromQuery] int? yearFrom = null,
            [FromQuery] int? yearTo = null,
            [FromQuery] string? vehicleType = null)
        {
            var result = await _svc.GetPagedAsync(page, pageSize, licensePlate, carMaker, model, status, yearFrom, yearTo, vehicleType);
            return Ok(result);
        }

        // ======================= [GET BY ID] =======================
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var data = await _svc.GetByIdAsync(id);
            return Ok(data);
        }

        // ======================= [CREATE] =======================
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] VehicleCreateDto dto)
        {
            var created = await _svc.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.VehicleId }, created);
        }

        // ======================= [UPDATE] =======================
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] VehicleUpdateDto dto)
        {
            await _svc.UpdateAsync(id, dto);
            return NoContent();
        }

        // ======================= [CHANGE STATUS] =======================
        // PATCH: /api/vehicles/{id}/status?status=Active
        [HttpPatch("{id:int}/status")]
        public async Task<IActionResult> ChangeStatus([FromRoute] int id, [FromQuery] string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return BadRequest(new { message = "Status không được để trống." });

            var value = status.Trim();
            if (value != "Active" && value != "Inactive" && value != "Blacklisted" && value != "Retired")
                return BadRequest(new { message = "Status chỉ nhận: Active / Inactive / Blacklisted / Retired." });

            await _svc.ChangeStatusAsync(id, value);
            return NoContent();
        }

        // ======================= [DELETE] =======================
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            await _svc.DeleteAsync(id);
            return NoContent();
        }

        // ======================= [UPLOAD IMAGE] =======================
        // POST: /api/vehicles/image/upload
        [HttpPost("image/upload")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(VehicleReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UploadImage([FromForm] VehicleImageUploadDto form)
        {
            try
            {
                var dto = await _svc.UploadImageAsync(form.VehicleId, form.File);
                return Ok(dto);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}