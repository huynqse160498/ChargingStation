using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Repositories.DTOs;
using Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IVnPayService _vnPayService;
        private readonly IConfiguration _config;

        public PaymentController(IPaymentService paymentService, IVnPayService vnPayService, IConfiguration config)
        {
            _paymentService = paymentService;
            _vnPayService = vnPayService;
            _config = config;
        }

        // =====================================================
        // 🔹 1️⃣ Tạo URL thanh toán (Booking / Invoice)
        // =====================================================
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] PaymentCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            var url = await _paymentService.CreatePaymentUrl(dto, ip);

            return Ok(new
            {
                success = true,
                message = "Tạo URL thanh toán thành công.",
                paymentUrl = url,
                type = dto.BookingId.HasValue ? "Booking" :
                        dto.InvoiceId.HasValue ? "Invoice" :
                        "Unknown"
            });
        }
        [HttpPost("create-combo-url")]
        public async Task<IActionResult> CreateComboUrl([FromBody] ComboPaymentRequest req)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            var url = await _paymentService.CreateComboPaymentUrl(req.InvoiceId, req.SubscriptionId, ip);
            return Ok(new { paymentUrl = url });
        }

     

        // =====================================================
        // 🔹 2️⃣ Tạo URL thanh toán cho Subscription (manual renew)
        // =====================================================
        [HttpPost("create-subscription/{subscriptionId}")]
        public async Task<IActionResult> CreateSubscriptionPayment([FromRoute] int subscriptionId)
        {
            if (subscriptionId <= 0)
                return BadRequest(new { success = false, message = "Thiếu subscriptionId hợp lệ." });

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            var url = await _paymentService.CreateSubscriptionPaymentUrl(subscriptionId, ip);

            return Ok(new
            {
                success = true,
                message = "Tạo URL thanh toán subscription thành công.",
                paymentUrl = url,
                subscriptionId
            });
        }

        // =====================================================
        // 🔹 3️⃣ Callback (GET) — VNPay redirect về sau thanh toán
        // =====================================================
        [HttpGet("vnpay-callback")]
        [AllowAnonymous]
        public async Task<IActionResult> VnPayCallback()
        {
            var successUrl = _config["VnPay:FrontEndSuccessUrl"] ?? "http://localhost:5173/payment/success";
            var failUrl = _config["VnPay:FrontEndFailUrl"] ?? "http://localhost:5173/payment/failure";

            try
            {
                var isValid = _vnPayService.ValidateResponse(Request.Query, out var txnRef);
                var code = Request.Query["vnp_ResponseCode"].ToString();
                var status = Request.Query["vnp_TransactionStatus"].ToString();

                if (isValid && code == "00" && status == "00")
                {
                    var msg = await _paymentService.HandleCallbackAsync(Request.Query);
                    var orderInfo = Request.Query["vnp_OrderInfo"].ToString();

                    string type = "Unknown";
                    int id = 0;

                    if (orderInfo.Contains("booking", StringComparison.OrdinalIgnoreCase))
                    {
                        type = "Booking";
                        int.TryParse(orderInfo.Split('#')[1], out id);
                    }
                    else if (orderInfo.Contains("hóa đơn", StringComparison.OrdinalIgnoreCase) ||
                             orderInfo.Contains("invoice", StringComparison.OrdinalIgnoreCase))
                    {
                        type = "Invoice";
                        int.TryParse(orderInfo.Split('#')[1], out id);
                    }
                    else if (orderInfo.Contains("subscription", StringComparison.OrdinalIgnoreCase))
                    {
                        type = "Subscription";
                        int.TryParse(orderInfo.Split('#')[1], out id);
                    }

                    return Redirect($"{successUrl}?type={type}&id={id}&txnRef={txnRef}&success=true");
                }

                return Redirect($"{failUrl}?success=false&reason=payment_failed");
            }
            catch (Exception ex)
            {
                return Redirect($"{failUrl}?success=false&reason={Uri.EscapeDataString(ex.Message)}");
            }
        }

        // =====================================================
        // 🔹 4️⃣ Callback POST — webhook nội bộ (tuỳ chọn)
        // =====================================================
        [HttpPost("vnpay-callback")]
        [AllowAnonymous]
        public Task<IActionResult> VnPayCallbackPost() => VnPayCallback();

        // =====================================================
        // 🔹 5️⃣ API test (giả lập callback — sandbox/local)
        // =====================================================
        [HttpPost("vnpay-callback-test")]
        public async Task<IActionResult> TestCallback([FromBody] PaymentCreateDto dto)
        {
            if (dto.BookingId == null && dto.InvoiceId == null && dto.SubscriptionId == null)
                return BadRequest(new { success = false, message = "Thiếu BookingId, InvoiceId hoặc SubscriptionId." });

            string orderInfo = dto.BookingId.HasValue
                ? $"Thanh toán booking #{dto.BookingId}"
                : dto.InvoiceId.HasValue
                    ? $"Thanh toán hóa đơn #{dto.InvoiceId}"
                    : $"Thanh toán subscription #{dto.SubscriptionId}";

            var fakeQuery = new QueryCollection(new System.Collections.Generic.Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                { "vnp_ResponseCode", "00" },
                { "vnp_TransactionStatus", "00" },
                { "vnp_OrderInfo", orderInfo },
                { "vnp_Amount", "200000" },
                { "vnp_SecureHash", "FAKEHASH" },
                { "vnp_TxnRef", Guid.NewGuid().ToString("N").Substring(0,10) }
            });

            var msg = await _paymentService.HandleCallbackAsync(fakeQuery);

            string type = dto.BookingId.HasValue ? "Booking" :
                          dto.InvoiceId.HasValue ? "Invoice" :
                          "Subscription";

            return Ok(new { success = true, message = msg, type });
        }
   
    }
}
