using Microsoft.AspNetCore.Http;
using Repositories.DTOs;
using Repositories.Interfaces;
using Repositories.Models;
using Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace Services.Implementations
{
    public class PaymentService : IPaymentService
    {
        private readonly IVnPayService _vnPay;
        private readonly IPaymentRepository _paymentRepo;
        private readonly IBookingRepository _bookingRepo;
        private readonly IInvoiceRepository _invoiceRepo;

        public PaymentService(
            IVnPayService vnPay,
            IPaymentRepository paymentRepo,
            IBookingRepository bookingRepo,
            IInvoiceRepository invoiceRepo)
        {
            _vnPay = vnPay;
            _paymentRepo = paymentRepo;
            _bookingRepo = bookingRepo;
            _invoiceRepo = invoiceRepo;
        }

        // 🔹 Tạo URL thanh toán (Booking hoặc Invoice)
        public async Task<string> CreatePaymentUrl(PaymentCreateDto dto, string ipAddress)
        {
            var txnRef = Guid.NewGuid().ToString("N").Substring(0, 10);
            return await _vnPay.CreatePaymentUrl(dto, ipAddress, txnRef);
        }

        // 🔹 Xử lý callback từ VNPay
        public async Task<string> HandleCallbackAsync(IQueryCollection query)
        {
            if (!_vnPay.ValidateResponse(query, out string txnRef))
                return "❌ Chữ ký không hợp lệ.";

            var code = query["vnp_ResponseCode"].ToString();
            var status = query["vnp_TransactionStatus"].ToString();

            if (code != "00" || status != "00")
                return $"⚠️ Thanh toán thất bại (Mã lỗi {code}).";

            // 🔍 Lấy thông tin đơn hàng từ callback
            var orderInfo = query["vnp_OrderInfo"].ToString();
            int.TryParse(orderInfo.Split('#').LastOrDefault(), out int id);

            var payment = new Payment
            {
                Method = "VNPAY",
                Status = "Success",
                PaidAt = DateTime.Now,
                CreatedAt = DateTime.Now
            };

            // 🧾 Nếu là Booking
            if (orderInfo.Contains("booking"))
            {
                var booking = await _bookingRepo.GetByIdAsync(id)
                    ?? throw new Exception($"Không tìm thấy booking #{id}.");

                payment.BookingId = booking.BookingId;
                payment.CustomerId = booking.CustomerId ?? 0;
                payment.Amount = booking.Price ?? 0;

                booking.Status = "Confirmed";
                booking.UpdatedAt = DateTime.Now;
                await _bookingRepo.SaveAsync();

                await _paymentRepo.AddAsync(payment);
                await _paymentRepo.SaveAsync();

                return $"✅ Thanh toán thành công cho Booking #{booking.BookingId}.";
            }

            // 🧾 Nếu là Invoice
            if (orderInfo.Contains("hóa đơn") || orderInfo.Contains("invoice"))
            {
                var invoice = await _invoiceRepo.GetByIdAsync(id)
                    ?? throw new Exception($"Không tìm thấy hóa đơn #{id}.");

                payment.InvoiceId = invoice.InvoiceId;
                payment.CustomerId = invoice.CustomerId ?? 0;
                payment.Amount = invoice.Total ?? 0;

                invoice.Status = "Paid";
                invoice.UpdatedAt = DateTime.Now;
                await _invoiceRepo.UpdateAsync(invoice);

                await _paymentRepo.AddAsync(payment);
                await _paymentRepo.SaveAsync();

                return $"✅ Thanh toán thành công cho hóa đơn #{invoice.InvoiceId}.";
            }

            return "❓ Không xác định được loại giao dịch.";
        }
    }
}
