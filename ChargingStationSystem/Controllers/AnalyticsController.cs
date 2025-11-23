using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // ✅ bắt buộc đăng nhập
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _svc;
        public AnalyticsController(IAnalyticsService svc) => _svc = svc;

        // ── helpers ─────────────────────────────────────────────────────────────
        private static (int month, int year) ResolveMonthYear(int? month, int? year)
        {
            var now = DateTime.UtcNow.AddHours(7);
            return (month is > 0 and <= 12 ? month.Value : now.Month,
                    year is >= 2000 ? year.Value : now.Year);
        }

        private bool IsAdmin() => User.IsInRole("Admin");

        private int? GetCompanyIdFromClaims()
        {
            var v = User.FindFirst("CompanyId")?.Value;
            return int.TryParse(v, out var id) ? id : null;
        }

        // =============== PHẦN COMPANY ĐƯỢC XEM (chỉ công ty của mình) ===============

        // 1) Summary (Company & Admin)
        [HttpGet("summary")]
        [Authorize(Roles = "Admin,Company")]
        public async Task<IActionResult> GetSummary([FromQuery] int? month, [FromQuery] int? year, [FromQuery] int? companyId = null)
        {
            var (m, y) = ResolveMonthYear(month, year);

            if (IsAdmin())
            {
                // Admin: cho phép truyền companyId hoặc để null (all)
                var dataAdmin = await _svc.GetSummaryAsync(m, y, companyId, adminView: true);
                return Ok(dataAdmin);
            }
            else
            {
                // Company: ép về companyId trong token
                var cid = GetCompanyIdFromClaims();
                if (cid is null) return Forbid();
                var dataSelf = await _svc.GetSummaryAsync(m, y, cid, adminView: false);
                return Ok(dataSelf);
            }
        }

        // 2) Revenue Sources (Company & Admin)
        [HttpGet("revenue-sources")]
        [Authorize(Roles = "Admin,Company")]
        public async Task<IActionResult> GetRevenueSources([FromQuery] int? month, [FromQuery] int? year, [FromQuery] int? companyId = null)
        {
            var (m, y) = ResolveMonthYear(month, year);

            if (IsAdmin())
            {
                var dataAdmin = await _svc.GetRevenueSourcesAsync(m, y, companyId, adminView: true);
                return Ok(dataAdmin);
            }
            else
            {
                var cid = GetCompanyIdFromClaims();
                if (cid is null) return Forbid();
                var dataSelf = await _svc.GetRevenueSourcesAsync(m, y, cid, adminView: false);
                return Ok(dataSelf);
            }
        }

        // 3) Breakdown theo Vehicle (Company: của mình; Admin: xem công ty bất kỳ)
        [HttpGet("breakdown/vehicle")]
        [Authorize(Roles = "Admin,Company")]
        public async Task<IActionResult> GetBreakdownByVehicle([FromQuery] int? month, [FromQuery] int? year, [FromQuery] int? companyId = null)
        {
            var (m, y) = ResolveMonthYear(month, year);

            if (IsAdmin())
            {
                if (companyId is null) return BadRequest(new { message = "Admin cần truyền companyId." });
                var dataAdmin = await _svc.GetBreakdownByVehicleAsync(m, y, companyId.Value);
                return Ok(dataAdmin);
            }
            else
            {
                var cid = GetCompanyIdFromClaims();
                if (cid is null) return Forbid();
                var dataSelf = await _svc.GetBreakdownByVehicleAsync(m, y, cid.Value);
                return Ok(dataSelf);
            }
        }

        // =============== PHẦN CHỈ ADMIN ĐƯỢC XEM ================================

        [HttpGet("breakdown/company")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetBreakdownByCompany([FromQuery] int? month, [FromQuery] int? year)
        {
            var (m, y) = ResolveMonthYear(month, year);
            return Ok(await _svc.GetBreakdownByCompanyAsync(m, y));
        }

        [HttpGet("breakdown/stations")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetBreakdownByStation([FromQuery] int? month, [FromQuery] int? year, [FromQuery] int? companyId = null)
        {
            var (m, y) = ResolveMonthYear(month, year);
            return Ok(await _svc.GetBreakdownByStationAsync(m, y, companyId, adminView: true));
        }

        [HttpGet("breakdown/chargers")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetBreakdownByCharger([FromQuery] int? month, [FromQuery] int? year, [FromQuery] int? companyId = null)
        {
            var (m, y) = ResolveMonthYear(month, year);
            return Ok(await _svc.GetBreakdownByChargerAsync(m, y, companyId, adminView: true));
        }

        [HttpGet("breakdown/ports")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetBreakdownByPort([FromQuery] int? month, [FromQuery] int? year, [FromQuery] int? companyId = null)
        {
            var (m, y) = ResolveMonthYear(month, year);
            return Ok(await _svc.GetBreakdownByPortAsync(m, y, companyId, adminView: true));
        }

        [HttpGet("breakdown/connector-types")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetBreakdownByConnectorType([FromQuery] int? month, [FromQuery] int? year, [FromQuery] int? companyId = null)
        {
            var (m, y) = ResolveMonthYear(month, year);
            return Ok(await _svc.GetBreakdownByConnectorTypeAsync(m, y, companyId, adminView: true));
        }

        [HttpGet("breakdown/vehicle-types")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetBreakdownByVehicleType([FromQuery] int? month, [FromQuery] int? year, [FromQuery] int? companyId = null)
        {
            var (m, y) = ResolveMonthYear(month, year);
            return Ok(await _svc.GetBreakdownByVehicleTypeAsync(m, y, companyId, adminView: true));
        }

        [HttpGet("breakdown/time-range")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetBreakdownByTimeRange([FromQuery] int? month, [FromQuery] int? year, [FromQuery] int? companyId = null)
        {
            var (m, y) = ResolveMonthYear(month, year);
            return Ok(await _svc.GetBreakdownByTimeRangeAsync(m, y, companyId, adminView: true));
        }

        [HttpGet("utilization")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetHardwareUtilization(
            [FromQuery] string scope = "Port",
            [FromQuery] int? month = null,
            [FromQuery] int? year = null,
            [FromQuery] int? companyId = null,
            [FromQuery] double minUtilization = 0.05,
            [FromQuery] int minSessions = 3)
        {
            var (m, y) = ResolveMonthYear(month, year);
            return Ok(await _svc.GetHardwareUtilizationAsync(m, y, companyId, adminView: true, scope, minUtilization, minSessions));
        }

        [HttpGet("top-under")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetTopAndUnder(
            [FromQuery] int? month = null,
            [FromQuery] int? year = null,
            [FromQuery] int? companyId = null,
            [FromQuery] double minUtilization = 0.05,
            [FromQuery] int minSessions = 3)
        {
            var (m, y) = ResolveMonthYear(month, year);
            return Ok(await _svc.GetTopAndUnderAsync(m, y, companyId, adminView: true, minUtilization, minSessions));
        }
  

    }
}
