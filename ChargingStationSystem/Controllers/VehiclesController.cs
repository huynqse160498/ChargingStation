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
    [Produces("application/json")]
    public class VehiclesController : ControllerBase
    {
        private readonly IVehicleService _service;
        public VehiclesController(IVehicleService service) => _service = service;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _service.GetAllAsync();
            return Ok(data);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var item = await _service.GetByIdAsync(id);
                return Ok(item);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] VehicleCreateDto dto)
        {
            // NEW: dto có thêm CustomerId, CompanyId, CurrentSoc
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            try
            {
                var created = await _service.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.VehicleId }, created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] VehicleUpdateDto dto)
        {
            // NEW: dto có thêm CompanyId, CurrentSoc
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

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

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _service.DeleteAsync(id);
            return ok ? NoContent() : NotFound();
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? licensePlate = null,
            [FromQuery] string? carMaker = null,
            [FromQuery] string? model = null,
            [FromQuery] string? status = null,
            [FromQuery] int? yearFrom = null,
            [FromQuery] int? yearTo = null)
        {
            var (items, total) = await _service.GetPagedAsync(
                page, pageSize, licensePlate, carMaker, model, status, yearFrom, yearTo);

            return Ok(new { page, pageSize, total, items });
        }

        public class VehicleChangeStatusRequest { public string Status { get; set; } = string.Empty; }

        [HttpPatch("{id:int}/status")]
        public async Task<IActionResult> ChangeStatus(int id, [FromBody] VehicleChangeStatusRequest req)
        {
            if (string.IsNullOrWhiteSpace(req?.Status))
                return BadRequest(new { message = "Status không được trống." });

            var ok = await _service.ChangeStatusAsync(id, req.Status);
            return ok ? NoContent() : NotFound();
        }
    }
}