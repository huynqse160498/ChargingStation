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
            [FromQuery] string? vehicleType = null //NEW
        )
        {
            var result = await _svc.GetPagedAsync(page, pageSize, licensePlate, carMaker, model, status, yearFrom, yearTo, vehicleType);
            return Ok(result); // tránh mọi kiểu gán 'var x = Ok(...)' gây CS0815
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var data = await _svc.GetByIdAsync(id);
            return Ok(data);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] VehicleCreateDto dto)
        {
            var created = await _svc.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.VehicleId }, created);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] VehicleUpdateDto dto)
        {
            await _svc.UpdateAsync(id, dto);
            return NoContent();
        }

        [HttpPatch("{id:int}/status")]
        public async Task<IActionResult> ChangeStatus([FromRoute] int id, [FromQuery] string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return BadRequest("Status không được để trống.");
            await _svc.ChangeStatusAsync(id, status.Trim());
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            await _svc.DeleteAsync(id);
            return NoContent();
        }
    }
}