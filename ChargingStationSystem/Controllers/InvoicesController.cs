using Microsoft.AspNetCore.Mvc;
using Repositories.DTOs;
using Services.Interfaces;

namespace ChargingStationSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoicesController : ControllerBase
    {
        private readonly IInvoiceService _service;

        public InvoicesController(IInvoiceService service)
        {
            _service = service;
        }

        // ============================================================
        // 🔹 [ADMIN] Lấy tất cả hóa đơn
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var invoices = await _service.GetAllAsync();

            var result = invoices.Select(i => new
            {
                i.InvoiceId,
                i.CustomerId,
                i.CompanyId,
                i.BillingMonth,
                i.BillingYear,
                i.Status,
                i.Subtotal,
                i.Tax,
                i.Total,
                i.CreatedAt,
                i.UpdatedAt,
                i.DueDate,

                Subscription = i.Subscription == null ? null : new
                {
                    i.Subscription.SubscriptionId,
                    i.Subscription.StartDate,
                    i.Subscription.EndDate,
                    Plan = i.Subscription.SubscriptionPlan == null ? null : new
                    {
                        i.Subscription.SubscriptionPlan.PlanName,
                        i.Subscription.SubscriptionPlan.PriceMonthly,
                        i.Subscription.SubscriptionPlan.DiscountPercent,
                        i.Subscription.SubscriptionPlan.FreeIdleMinutes
                    }
                },

                ChargingSessions = i.ChargingSessions?.Select(cs => new
                {
                    cs.ChargingSessionId,
                    cs.VehicleId,
                    cs.PortId,
                    cs.Total,
                    cs.Status,
                    cs.StartedAt,
                    cs.EndedAt
                })
            });

            return Ok(new
            {
                message = "✅ Danh sách hóa đơn",
                total = result.Count(),
                data = result
            });
        }

        // ============================================================
        // 🔹 [CUSTOMER] Xem hóa đơn của chính mình
        // ============================================================
        [HttpGet("by-customer/{customerId}")]
        public async Task<IActionResult> GetByCustomer(int customerId)
        {
            var invoices = await _service.GetByCustomerIdAsync(customerId);
            if (!invoices.Any())
                return NotFound(new { message = "Khách hàng chưa có hóa đơn nào." });

            var result = invoices.Select(i => new
            {
                i.InvoiceId,
                i.BillingMonth,
                i.BillingYear,
                i.Status,
                i.Total,
                i.CreatedAt,
                i.DueDate,
                SubscriptionPlan = i.Subscription?.SubscriptionPlan?.PlanName
            });

            return Ok(new
            {
                message = $"✅ {invoices.Count} hóa đơn của khách hàng #{customerId}",
                data = result
            });
        }

        // ============================================================
        // 🔹 [COMPANY] Xem tất cả hóa đơn của nhân viên trong công ty
        // ============================================================
        [HttpGet("by-company/{companyId}")]
        public async Task<IActionResult> GetByCompany(int companyId)
        {
            var invoices = await _service.GetByCompanyIdAsync(companyId);
            if (!invoices.Any())
                return NotFound(new { message = "Công ty này chưa có hóa đơn nào." });

            var result = invoices.Select(i => new
            {
                i.InvoiceId,
                i.BillingMonth,
                i.BillingYear,
                i.Status,
                i.Total,
                i.CreatedAt,
                i.DueDate,
                SubscriptionPlan = i.Subscription?.SubscriptionPlan?.PlanName
            });

            return Ok(new
            {
                message = $"✅ {invoices.Count} hóa đơn của công ty #{companyId}",
                data = result
            });
        }

        // ============================================================
        // 🔹 Xem chi tiết hóa đơn (có Subscription + Plan)
        // ============================================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var invoice = await _service.GetByIdAsync(id);
            if (invoice == null)
                return NotFound(new { message = "❌ Không tìm thấy hóa đơn." });

            return Ok(new
            {
                message = "✅ Chi tiết hóa đơn",
                data = new
                {
                    invoice.InvoiceId,
                    invoice.CustomerId,
                    invoice.CompanyId,
                    invoice.BillingMonth,
                    invoice.BillingYear,
                    invoice.Status,
                    invoice.Subtotal,
                    invoice.Tax,
                    invoice.Total,
                    invoice.CreatedAt,
                    invoice.UpdatedAt,
                    invoice.DueDate,
                    Subscription = invoice.Subscription == null ? null : new
                    {
                        invoice.Subscription.SubscriptionId,
                        invoice.Subscription.StartDate,
                        invoice.Subscription.EndDate,
                        Plan = invoice.Subscription.SubscriptionPlan == null ? null : new
                        {
                            invoice.Subscription.SubscriptionPlan.PlanName,
                            invoice.Subscription.SubscriptionPlan.PriceMonthly,
                            invoice.Subscription.SubscriptionPlan.DiscountPercent,
                            invoice.Subscription.SubscriptionPlan.FreeIdleMinutes
                        }
                    },

                    ChargingSessions = invoice.ChargingSessions?.Select(cs => new
                    {
                        cs.ChargingSessionId,
                        cs.VehicleId,
                        cs.PortId,
                        cs.Total,
                        cs.Status,
                        cs.StartedAt,
                        cs.EndedAt
                    })
                }
            });
        }

        // ============================================================
        // 🔹 [ADMIN] Tạo hóa đơn thủ công (ngoài hóa đơn tự sinh)
        // ============================================================
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] InvoiceCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var invoice = await _service.CreateAsync(dto);
                return Ok(new
                {
                    message = "✅ Tạo hóa đơn thủ công thành công!",
                    data = new
                    {
                        invoice.InvoiceId,
                        invoice.Status,
                        invoice.Total,
                        invoice.CreatedAt
       
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.InnerException?.Message ?? ex.Message });
            }
        }

        // ============================================================
        // 🔹 [ADMIN] Cập nhật trạng thái (Paid / Overdue / Unpaid)
        // ============================================================
        [HttpPut("status")]
        public async Task<IActionResult> UpdateStatus([FromBody] InvoiceUpdateStatusDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _service.UpdateStatusAsync(dto);
                return Ok(new { message = $"✅ Cập nhật trạng thái hóa đơn #{dto.InvoiceId} thành công ({dto.Status})." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.InnerException?.Message ?? ex.Message });
            }
        }

        // ============================================================
        // 🔹 [ADMIN] Xóa hóa đơn
        // ============================================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _service.DeleteAsync(id);
                return Ok(new { message = "🗑️ Đã xóa hóa đơn thành công." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.InnerException?.Message ?? ex.Message });
            }
        }
    }
}
