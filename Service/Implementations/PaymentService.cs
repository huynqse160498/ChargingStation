using Microsoft.AspNetCore.Http;
using Repositories.DTOs;
using Repositories.Interfaces;
using Repositories.Models;
using Services.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Services.Implementations
{
    public class PaymentService : IPaymentService
    {
        private readonly IVnPayService _vnPay;
        private readonly IPaymentRepository _paymentRepo;
        private readonly IBookingRepository _bookingRepo;
        private readonly IInvoiceRepository _invoiceRepo;
        private readonly ISubscriptionRepository _subscriptionRepo;
        private readonly ISubscriptionPlanRepository _planRepo;

        public PaymentService(
            IVnPayService vnPay,
            IPaymentRepository paymentRepo,
            IBookingRepository bookingRepo,
            IInvoiceRepository invoiceRepo,
            ISubscriptionRepository subscriptionRepo,
            ISubscriptionPlanRepository planRepo)
        {
            _vnPay = vnPay;
            _paymentRepo = paymentRepo;
            _bookingRepo = bookingRepo;
            _invoiceRepo = invoiceRepo;
            _subscriptionRepo = subscriptionRepo;
            _planRepo = planRepo;
        }

        // ===========================================================
        // 🔹 1️⃣ Tạo payment URL cho Booking hoặc Invoice
        // ===========================================================
        public async Task<string> CreatePaymentUrl(PaymentCreateDto dto, string ipAddress)
        {
            var txnRef = Guid.NewGuid().ToString("N").Substring(0, 10);
            string orderInfo;

            if (dto.BookingId.HasValue)
                orderInfo = $"Thanh toán booking #{dto.BookingId}";
            else if (dto.InvoiceId.HasValue)
                orderInfo = $"Thanh toán hóa đơn #{dto.InvoiceId}";
            else
                throw new Exception("Phải có BookingId hoặc InvoiceId để tạo thanh toán.");

            dto.Description = orderInfo;
            return await _vnPay.CreatePaymentUrl(dto, ipAddress, txnRef);
        }

        // ===========================================================
        // 🔹 2️⃣ Tạo payment URL riêng cho Subscription (manual renew)
        // ===========================================================
        public async Task<string> CreateSubscriptionPaymentUrl(int subscriptionId, string ipAddress)
        {
            var sub = await _subscriptionRepo.GetByIdAsync(subscriptionId)
                ?? throw new Exception($"Không tìm thấy Subscription #{subscriptionId}.");

            var plan = await _planRepo.GetByIdAsync(sub.SubscriptionPlanId)
                ?? throw new Exception("Không tìm thấy gói Subscription.");

            var txnRef = Guid.NewGuid().ToString("N").Substring(0, 10);
            string orderInfo = $"Thanh toán subscription #{subscriptionId}";

            var dto = new PaymentCreateDto
            {
                // ⚠️ Các thuộc tính này cần có trong PaymentCreateDto
                SubscriptionId = subscriptionId,
                Description = orderInfo,
            };

            return await _vnPay.CreatePaymentUrl(dto, ipAddress, txnRef);
        }

        // ===========================================================
        // 🔹 3️⃣ Xử lý callback (Booking / Invoice / Subscription)
        // ===========================================================
        public async Task<string> HandleCallbackAsync(IQueryCollection query)
        {
            if (!_vnPay.ValidateResponse(query, out string txnRef))
                return "❌ Chữ ký VNPay không hợp lệ.";

            var code = query["vnp_ResponseCode"].ToString();
            var status = query["vnp_TransactionStatus"].ToString();
            if (code != "00" || status != "00")
                return $"⚠️ Thanh toán thất bại (Mã lỗi {code}).";

            string orderInfo = query["vnp_OrderInfo"].ToString().ToLower();
            int.TryParse(orderInfo.Split('#').LastOrDefault(), out int id);

            if (orderInfo.Contains("booking"))
                return await HandleBookingPaymentAsync(id);
            if (orderInfo.Contains("invoice") || orderInfo.Contains("hóa đơn"))
                return await HandleInvoicePaymentAsync(id);
            if (orderInfo.Contains("subscription"))
                return await HandleSubscriptionPaymentAsync(id);

            return "❓ Không xác định được loại giao dịch.";
        }

        // ===========================================================
        // 🔹 4️⃣ Xử lý thanh toán Booking
        // ===========================================================
        private async Task<string> HandleBookingPaymentAsync(int bookingId)
        {
            var booking = await _bookingRepo.GetByIdAsync(bookingId)
                ?? throw new Exception($"Không tìm thấy Booking #{bookingId}.");

            var payment = new Payment
            {
                BookingId = booking.BookingId,
                CustomerId = booking.CustomerId ?? 0,
                Amount = booking.Price,
                Method = "VNPAY",
                Status = "Success",
                PaidAt = DateTime.Now,
                CreatedAt = DateTime.Now
            };

            booking.Status = "Confirmed";
            booking.UpdatedAt = DateTime.Now;

            await _bookingRepo.SaveAsync();
            await _paymentRepo.AddAsync(payment);

            return $"✅ Thanh toán thành công cho Booking #{booking.BookingId}.";
        }

        // ===========================================================
        // 🔹 5️⃣ Xử lý thanh toán Invoice
        // ===========================================================
        private async Task<string> HandleInvoicePaymentAsync(int invoiceId)
        {
            var invoice = await _invoiceRepo.GetByIdAsync(invoiceId)
                ?? throw new Exception($"Không tìm thấy Hóa đơn #{invoiceId}.");

            var payment = new Payment
            {
                InvoiceId = invoice.InvoiceId,
                CustomerId = invoice.CustomerId ?? 0,
                Amount = invoice.Total,
                Method = "VNPAY",
                Status = "Success",
                PaidAt = DateTime.Now,
                CreatedAt = DateTime.Now
            };

            invoice.Status = "Paid";
            invoice.UpdatedAt = DateTime.Now;

            await _invoiceRepo.UpdateAsync(invoice);
            await _paymentRepo.AddAsync(payment);

            return $"✅ Thanh toán thành công cho Hóa đơn #{invoice.InvoiceId}.";
        }

        // ===========================================================
        // 🔹 6️⃣ Xử lý thanh toán Subscription (manual renew)
        // ===========================================================
        private async Task<string> HandleSubscriptionPaymentAsync(int subscriptionId)
        {
            var sub = await _subscriptionRepo.GetByIdAsync(subscriptionId)
                ?? throw new Exception($"Không tìm thấy Subscription #{subscriptionId}.");

            var plan = await _planRepo.GetByIdAsync(sub.SubscriptionPlanId)
                ?? throw new Exception("Không tìm thấy gói Subscription.");

            decimal amount = plan.PriceMonthly;

            var payment = new Payment
            {
                SubscriptionId = sub.SubscriptionId,
                CustomerId = sub.CustomerId,
                Amount = amount,
                Method = "VNPAY",
                Status = "Success",
                PaidAt = DateTime.Now,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            // ✅ Chỉ gia hạn nếu chưa hoạt động hoặc đã hết hạn
            if (sub.Status != "Active" || sub.EndDate == null || sub.EndDate < DateTime.Now)
            {
                sub.StartDate = DateTime.Now;
                sub.EndDate = DateTime.Now.AddMonths(1);
                sub.NextBillingDate = sub.EndDate;
                sub.Status = "Active";
                sub.UpdatedAt = DateTime.Now;
                await _subscriptionRepo.UpdateAsync(sub);
            }

            await _paymentRepo.AddAsync(payment);
            await _paymentRepo.SaveAsync();

            // 🧾 Tạo hóa đơn riêng cho Subscription
            var invoice = new Invoice
            {
                CustomerId = sub.CustomerId,
                SubscriptionId = sub.SubscriptionId,
                BillingMonth = DateTime.Now.Month,
                BillingYear = DateTime.Now.Year,
                Subtotal = amount,
                Tax = Math.Round(amount * 0.1M, 2),
                Total = Math.Round(amount * 1.1M, 2),
                Status = "Paid",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                IsMonthlyInvoice = false
            };
            await _invoiceRepo.AddAsync(invoice);

            return $"✅ Thanh toán & gia hạn thành công Subscription #{sub.SubscriptionId} đến {sub.EndDate:dd/MM/yyyy}.";
        }

    }
}
