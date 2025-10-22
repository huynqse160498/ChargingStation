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
        // 🔹 1️⃣ Tạo URL thanh toán (Booking / Invoice / Company)
        // =====================================================
        [HttpPost("create")]
        public IActionResult Create([FromBody] PaymentCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            var url = _paymentService.CreatePaymentUrl(dto, ip);

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

        // =====================================================
        // 🔹 2️⃣ Callback (GET) — VNPay redirect về sau thanh toán
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

                    // ✅ Tách loại giao dịch và ID
                    var orderInfo = Request.Query["vnp_OrderInfo"].ToString();
                    string type = "Unknown";
                    int id = 0;

                    if (orderInfo.Contains("booking"))
                    {
                        type = "Booking";
                        int.TryParse(orderInfo.Split('#')[1], out id);
                    }
                    else if (orderInfo.Contains("hóa đơn") || orderInfo.Contains("invoice"))
                    {
                        type = "Invoice";
                        int.TryParse(orderInfo.Split('#')[1], out id);
                    }

                    // Redirect sang FE
                    return Redirect($"{successUrl}?type={type}&id={id}&txnRef={txnRef}&success=true");
                }

                // ❌ Nếu thanh toán thất bại
                return Redirect($"{failUrl}?success=false&reason=payment_failed");
            }
            catch (Exception ex)
            {
                // ❌ Nếu BE có lỗi
                return Redirect($"{failUrl}?success=false&reason={Uri.EscapeDataString(ex.Message)}");
            }
        }

        // =====================================================
        // 🔹 3️⃣ Callback POST — VNPay gọi webhook nội bộ (tuỳ chọn)
        // =====================================================
        [HttpPost("vnpay-callback")]
        [AllowAnonymous]
        public Task<IActionResult> VnPayCallbackPost() => VnPayCallback();

        // =====================================================
        // 🔹 4️⃣ API test (giả lập callback — test sandbox/local)
        // =====================================================
        [HttpPost("vnpay-callback-test")]
        public async Task<IActionResult> TestCallback([FromBody] PaymentCreateDto dto)
        {
            if (dto.BookingId == null && dto.InvoiceId == null)
                return BadRequest(new { success = false, message = "Thiếu BookingId hoặc InvoiceId." });

            string orderInfo = dto.BookingId.HasValue
                ? $"Thanh toán booking #{dto.BookingId}"
                : $"Thanh toán hóa đơn #{dto.InvoiceId}";

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

            return Ok(new
            {
                success = true,
                message = msg,
                type = dto.BookingId.HasValue ? "Booking" : "Invoice"
            });
        }
    }
}
