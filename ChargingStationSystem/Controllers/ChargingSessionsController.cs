using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repositories.DTOs;
using Services.Interfaces;

namespace ChargingStationSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChargingSessionsController : ControllerBase
    {
        private readonly IChargingSessionService _service;

        public ChargingSessionsController(IChargingSessionService service)
        {
            _service = service;
        }

        // =============================================
        // 🔹 Bắt đầu phiên sạc
        // =============================================
        [HttpPost("start")]
        public async Task<IActionResult> Start([FromBody] ChargingSessionCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var session = await _service.StartSessionAsync(dto);

                string msg = dto.BookingId != null
                    ? "✅ Bắt đầu phiên sạc theo Booking thành công!"
                    : "⚡ Bắt đầu phiên sạc trực tiếp thành công!";

                return Ok(new
                {
                    message = msg,
                    data = new
                    {
                        session.ChargingSessionId,
                        session.PortId,
                        session.VehicleId,
                        session.CustomerId,
                        session.CompanyId,
                        session.Status,
                        session.StartSoc,
                        session.StartedAt,
                        session.PricingRuleId,
                        VehicleType = session.Vehicle?.VehicleType,
                        PortStatus = session.Port?.Status,
                        ChargerType = session.Port?.Charger?.Type,
                        ChargerPowerKw = session.Port?.Charger?.PowerKw
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.InnerException?.Message ?? ex.Message });
            }
        }

        // =============================================
        // 🔹 Kết thúc phiên sạc (đã có subscription hiển thị)
        // =============================================
        [HttpPost("end")]
        public async Task<IActionResult> End([FromBody] ChargingSessionEndDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var session = await _service.EndSessionAsync(dto);

                return Ok(new
                {
                    message = "✅ Phiên sạc đã kết thúc thành công!",
                    data = new
                    {
                        session.ChargingSessionId,
                        session.CustomerId,
                        session.CompanyId,
                        session.VehicleId,
                        session.PortId,
                        session.StartSoc,
                        session.EndSoc,
                        session.EnergyKwh,
                        session.DurationMin,
                        session.IdleMin,
                        session.Subtotal,
                        session.Tax,
                        session.Total,
                        session.EndedAt,
                        session.Status,
                        BillingMonth = session.EndedAt?.Month,
                        BillingYear = session.EndedAt?.Year,
                        // ⚡ Thêm Invoice + Subscription hiển thị chi tiết
                        Invoice = session.Invoice == null ? null : new
                        {
                            session.Invoice.InvoiceId,
                            session.Invoice.Status,
                            session.Invoice.Total,
                            Subscription = session.Invoice.Subscription == null ? null : new
                            {
                                session.Invoice.Subscription.SubscriptionId,
                                PlanName = session.Invoice.Subscription.SubscriptionPlan?.PlanName,
                                DiscountPercent = session.Invoice.Subscription.SubscriptionPlan?.DiscountPercent,
                                FreeIdleMinutes = session.Invoice.Subscription.SubscriptionPlan?.FreeIdleMinutes
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.InnerException?.Message ?? ex.Message });
            }
        }

        // =============================================
        // 🔹 Lấy toàn bộ phiên sạc (có invoice + subscription)
        // =============================================
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var sessions = await _service.GetAllAsync();

            var result = sessions.Select(s => new
            {
                s.ChargingSessionId,
                s.CustomerId,
                s.CompanyId,
                s.PortId,
                s.VehicleId,
                s.StartedAt,
                s.EndedAt,
                s.Status,
                s.Total,
                Invoice = s.Invoice == null ? null : new
                {
                    s.Invoice.InvoiceId,
                    s.Invoice.Status,
                    s.Invoice.Total,
                    Subscription = s.Invoice.Subscription == null ? null : new
                    {
                        s.Invoice.Subscription.SubscriptionId,
                        PlanName = s.Invoice.Subscription.SubscriptionPlan?.PlanName,
                        DiscountPercent = s.Invoice.Subscription.SubscriptionPlan?.DiscountPercent,
                        FreeIdleMinutes = s.Invoice.Subscription.SubscriptionPlan?.FreeIdleMinutes
                    }
                }
            });

            return Ok(new { count = result.Count(), items = result });
        }

        // =============================================
        // 🔹 Lấy chi tiết 1 phiên sạc (có invoice + subscription)
        // =============================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var session = await _service.GetByIdAsync(id);
            if (session == null)
                return NotFound(new { message = "Không tìm thấy phiên sạc." });

            return Ok(new
            {
                session.ChargingSessionId,
                session.CustomerId,
                session.CompanyId,
                session.VehicleId,
                session.PortId,
                session.PricingRuleId,
                session.StartSoc,
                session.EndSoc,
                session.EnergyKwh,
                session.Subtotal,
                session.Tax,
                session.Total,
                session.DurationMin,
                session.IdleMin,
                session.StartedAt,
                session.EndedAt,
                session.Status,
                Invoice = session.Invoice == null ? null : new
                {
                    session.Invoice.InvoiceId,
                    session.Invoice.Status,
                    session.Invoice.Total,
                    Subscription = session.Invoice.Subscription == null ? null : new
                    {
                        session.Invoice.Subscription.SubscriptionId,
                        PlanName = session.Invoice.Subscription.SubscriptionPlan?.PlanName,
                        DiscountPercent = session.Invoice.Subscription.SubscriptionPlan?.DiscountPercent,
                        FreeIdleMinutes = session.Invoice.Subscription.SubscriptionPlan?.FreeIdleMinutes
                    }
                }
            });
        }

        // =============================================
        // 🔹 Xóa phiên sạc
        // =============================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _service.DeleteAsync(id);
                return Ok(new { message = "🗑️ Đã xóa phiên sạc thành công." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.InnerException?.Message ?? ex.Message });
            }
        }
    }
}
