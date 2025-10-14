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

        // ====================== 1️⃣ Tạo thanh toán ======================
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
                paymentUrl = url
            });
        }

        // ====================== 2️⃣ Xử lý callback thực tế (Redirect sang FE) ======================
        [HttpGet("vnpay-callback")]
        [AllowAnonymous]
        public async Task<IActionResult> VnPayCallback()
        {
            // 🟢 URL Frontend (đặt cứng hoặc lấy từ appsettings.json)
            var successUrl = _config["VnPay:FrontEndSuccessUrl"] ?? "http://localhost:5173/payment/success";
            var failUrl = _config["VnPay:FrontEndFailUrl"] ?? "http://localhost:5173/payment/failluer";

            try
            {
                var isValid = _vnPayService.ValidateResponse(Request.Query, out var txnRef);
                var code = Request.Query["vnp_ResponseCode"].ToString();
                var status = Request.Query["vnp_TransactionStatus"].ToString();

                if (isValid && code == "00" && status == "00")
                {
                    var msg = await _paymentService.HandleCallbackAsync(Request.Query);

                    // ✅ Lấy bookingId từ vnp_OrderInfo
                    var orderInfo = Request.Query["vnp_OrderInfo"].ToString();
                    int bookingId = 0;
                    if (orderInfo.Contains('#'))
                        int.TryParse(orderInfo.Split('#')[1], out bookingId);

                    // ✅ Redirect về FE khi thanh toán thành công
                    return Redirect($"{successUrl}?order={bookingId}&txnRef={txnRef}&success=true");
                }

                // ❌ Redirect về FE khi thất bại
                return Redirect($"{failUrl}?success=false&reason=payment_failed");
            }
            catch (Exception ex)
            {
                // ❌ Nếu lỗi trong BE → redirect về FE luôn
                return Redirect($"{failUrl}?success=false&reason={Uri.EscapeDataString(ex.Message)}");
            }
        }

        // ====================== 3️⃣ VNPay gọi POST (optional) ======================
        [HttpPost("vnpay-callback")]
        [AllowAnonymous]
        public Task<IActionResult> VnPayCallbackPost() => VnPayCallback();

        // ====================== 4️⃣ API Test callback (sandbox/local test) ======================
        [HttpPost("vnpay-callback-test")]
        public async Task<IActionResult> TestCallback([FromBody] int bookingId)
        {
            if (bookingId <= 0)
                return BadRequest(new { success = false, message = "Thiếu hoặc sai BookingId" });

            // Giả lập callback thành công
            var fakeQuery = new QueryCollection(new System.Collections.Generic.Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                { "vnp_ResponseCode", "00" },
                { "vnp_TransactionStatus", "00" },
                { "vnp_OrderInfo", $"Booking#{bookingId}" },
                { "vnp_Amount", "200000" },
                { "vnp_SecureHash", "FAKEHASH" },
                { "vnp_TxnRef", Guid.NewGuid().ToString("N").Substring(0,10) }
            });

            var msg = await _paymentService.HandleCallbackAsync(fakeQuery);

            return Ok(new
            {
                success = true,
                message = msg,
                bookingId
            });
        }
    }
}
