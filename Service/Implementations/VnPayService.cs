using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Repositories.DTOs;
using Repositories.Interfaces;
using Services.Interfaces;

namespace Services.Implementations
{
    public class VnPayService : IVnPayService
    {
        private readonly IConfiguration _config;
        private readonly IBookingRepository _bookingRepo;
        private readonly IInvoiceRepository _invoiceRepo;
        private readonly ILogger<VnPayService> _logger;

        public VnPayService(
            IConfiguration config,
            IBookingRepository bookingRepo,
            IInvoiceRepository invoiceRepo,
            ILogger<VnPayService> logger)
        {
            _config = config;
            _bookingRepo = bookingRepo;
            _invoiceRepo = invoiceRepo;
            _logger = logger;
        }

        // 🧾 Tạo URL thanh toán VNPay (Booking hoặc Invoice)
        public async Task<string> CreatePaymentUrl(PaymentCreateDto dto, string ipAddress, string txnRef)
        {
            decimal amount = 0;
            string orderInfo;

            if (dto.BookingId.HasValue)
            {
                var booking = await _bookingRepo.GetByIdAsync(dto.BookingId.Value)
                    ?? throw new Exception($"Không tìm thấy Booking #{dto.BookingId}");

                if (booking.Price == null)
                    throw new Exception("Booking chưa có giá để thanh toán.");

                amount = booking.Price.Value;
                orderInfo = $"Thanh toán booking #{booking.BookingId}";
            }
            else if (dto.InvoiceId.HasValue)
            {
                var invoice = await _invoiceRepo.GetByIdAsync(dto.InvoiceId.Value)
                    ?? throw new Exception($"Không tìm thấy Hóa đơn #{dto.InvoiceId}");

                if (invoice.Total == null || invoice.Total <= 0)
                    throw new Exception("Hóa đơn không có tổng tiền để thanh toán.");

                amount = invoice.Total.Value;
                orderInfo = $"Thanh toán hóa đơn #{invoice.InvoiceId}";
            }
            else
            {
                throw new Exception("Thiếu BookingId hoặc InvoiceId khi tạo thanh toán.");
            }

            var tmnCode = _config["VnPay:TmnCode"];
            var secret = (_config["VnPay:HashSecret"] ?? string.Empty).Trim();
            var baseUrl = _config["VnPay:BaseUrl"];
            var returnUrl = _config["VnPay:ReturnUrl"];

            var now = DateTime.UtcNow.AddHours(7);
            var expire = now.AddMinutes(15);

            var vnpParams = new SortedDictionary<string, string>
            {
                ["vnp_Version"] = "2.1.0",
                ["vnp_Command"] = "pay",
                ["vnp_TmnCode"] = tmnCode,
                ["vnp_Amount"] = ((long)(amount * 100)).ToString(),
                ["vnp_CreateDate"] = now.ToString("yyyyMMddHHmmss"),
                ["vnp_ExpireDate"] = expire.ToString("yyyyMMddHHmmss"),
                ["vnp_CurrCode"] = "VND",
                ["vnp_IpAddr"] = string.IsNullOrWhiteSpace(ipAddress) ? "127.0.0.1" : ipAddress,
                ["vnp_Locale"] = "vn",
                ["vnp_OrderInfo"] = orderInfo,
                ["vnp_OrderType"] = "other",
                ["vnp_ReturnUrl"] = returnUrl,
                ["vnp_TxnRef"] = txnRef
            };

            // 🔐 Tạo chữ ký SHA512
            var signData = BuildDataToSign(vnpParams);
            var secureHash = ComputeHmacSha512(secret, signData);

            var query = string.Join("&", vnpParams.Select(kv => $"{kv.Key}={FormEncodeUpper(kv.Value)}"));
            var finalUrl = $"{baseUrl}?{query}&vnp_SecureHashType=HMACSHA512&vnp_SecureHash={secureHash}";

            _logger.LogInformation("[VNPay SEND] signData={signData}", signData);
            _logger.LogInformation("[VNPay SEND] secureHash={secureHash}", secureHash);
            _logger.LogInformation("[VNPay SEND] finalUrl={finalUrl}", finalUrl);

            return finalUrl;
        }

        // ✅ Kiểm tra tính hợp lệ callback từ VNPay
        public bool ValidateResponse(IQueryCollection vnpParams, out string txnRef)
        {
            txnRef = vnpParams["vnp_TxnRef"];
            if (!vnpParams.ContainsKey("vnp_SecureHash"))
                return false;

            var secret = (_config["VnPay:HashSecret"] ?? string.Empty).Trim();
            var fromVnp = vnpParams["vnp_SecureHash"].ToString();

            var data = vnpParams
                .Where(kv => kv.Key.StartsWith("vnp_", StringComparison.Ordinal))
                .Where(kv => kv.Key != "vnp_SecureHash" && kv.Key != "vnp_SecureHashType")
                .Where(kv => !string.IsNullOrEmpty(kv.Value))
                .ToDictionary(kv => kv.Key, kv => kv.Value.ToString());

            var signData = BuildDataToSign(data);
            var computed = ComputeHmacSha512(secret, signData);

            _logger.LogInformation("[VNPay RETURN] signData={signData}", signData);
            _logger.LogInformation("[VNPay RETURN] computed={computed}", computed);
            _logger.LogInformation("[VNPay RETURN] fromVNPay={fromVNPay}", fromVnp);

            return computed.Equals(fromVnp, StringComparison.InvariantCultureIgnoreCase);
        }

        // ==================== Helpers ====================

        private static string FormEncodeUpper(string? value)
        {
            var encoded = HttpUtility.UrlEncode(value ?? string.Empty, Encoding.UTF8) ?? string.Empty;
            var sb = new StringBuilder(encoded.Length);
            for (int i = 0; i < encoded.Length; i++)
            {
                char c = encoded[i];
                if (c == '%' && i + 2 < encoded.Length)
                {
                    sb.Append('%');
                    sb.Append(char.ToUpperInvariant(encoded[i + 1]));
                    sb.Append(char.ToUpperInvariant(encoded[i + 2]));
                    i += 2;
                }
                else sb.Append(c);
            }
            return sb.ToString();
        }

        private static string BuildDataToSign(IDictionary<string, string> parameters)
        {
            var sorted = new SortedDictionary<string, string>(parameters, StringComparer.Ordinal);
            return string.Join("&", sorted.Select(kv => $"{kv.Key}={FormEncodeUpper(kv.Value)}"));
        }

        private static string ComputeHmacSha512(string key, string data)
        {
            using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(key ?? string.Empty));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data ?? string.Empty));
            return BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant();
        }
    }
}
