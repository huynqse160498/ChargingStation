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

        public PaymentService(IVnPayService vnPay, IPaymentRepository paymentRepo, IBookingRepository bookingRepo)
        {
            _vnPay = vnPay;
            _paymentRepo = paymentRepo;
            _bookingRepo = bookingRepo;
        }

        public string CreatePaymentUrl(PaymentCreateDto dto, string ipAddress)
        {
            var txnRef = Guid.NewGuid().ToString("N").Substring(0, 10);
            return _vnPay.CreatePaymentUrl(dto, ipAddress, txnRef);
        }

        public async Task<string> HandleCallbackAsync(IQueryCollection query)
        {
            if (!_vnPay.ValidateResponse(query, out string txnRef))
                return "Chữ ký không hợp lệ.";

            var code = query["vnp_ResponseCode"].ToString();
            var status = query["vnp_TransactionStatus"].ToString();

            if (code != "00" || status != "00")
                return $"Thanh toán thất bại (Mã lỗi {code}).";

            // Parse bookingId từ vnp_OrderInfo
            var orderInfo = query["vnp_OrderInfo"].ToString(); // "Thanh toán booking #5"
            int bookingId = 0;
            if (orderInfo.Contains('#'))
                int.TryParse(orderInfo.Split('#')[1], out bookingId);

            var booking = await _bookingRepo.GetByIdAsync(bookingId);
            if (booking == null)
                return $"Không tìm thấy booking #{bookingId}.";

            // Lưu payment
            var payment = new Payment
            {
                BookingId = booking.BookingId,
                CustomerId = booking.CustomerId ?? 0,
                Amount = booking.Price ?? 0,
                Method = "VNPAY",
                Status = "Success",
                PaidAt = DateTime.Now,
                CreatedAt = DateTime.Now
            };
            await _paymentRepo.AddAsync(payment);
            await _paymentRepo.SaveAsync();

            // Cập nhật trạng thái booking
            booking.Status = "Confirmed"; 
            booking.UpdatedAt = DateTime.Now;
            await _bookingRepo.SaveAsync();

            return $"Thanh toán thành công cho Booking #{bookingId}.";
        }

    }
}
