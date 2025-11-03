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
        private readonly INotificationRepository _notiRepo;
        private readonly IChargingSessionRepository _chargingSessionRepo;
        public PaymentService(
            IVnPayService vnPay,
            IPaymentRepository paymentRepo,
            IBookingRepository bookingRepo,
            IInvoiceRepository invoiceRepo,
            ISubscriptionRepository subscriptionRepo,
            ISubscriptionPlanRepository planRepo,
            INotificationRepository notiRepo,
            IChargingSessionRepository chargingSessionRepo)
        {
            _vnPay = vnPay;
            _paymentRepo = paymentRepo;
            _bookingRepo = bookingRepo;
            _invoiceRepo = invoiceRepo;
            _subscriptionRepo = subscriptionRepo;
            _planRepo = planRepo;
            _notiRepo = notiRepo;
            _chargingSessionRepo = chargingSessionRepo;
        }

        // ===========================================================
        // 🔹 1️⃣ Tạo payment URL cho Booking / Invoice / Subscription
        // ===========================================================
        public async Task<string> CreatePaymentUrl(PaymentCreateDto dto, string ipAddress)
        {
            var txnRef = Guid.NewGuid().ToString("N").Substring(0, 10);
            string orderInfo;

            if (dto.BookingId.HasValue)
                orderInfo = $"Thanh toán booking #{dto.BookingId}";
            else if (dto.InvoiceId.HasValue)
                orderInfo = $"Thanh toán hóa đơn #{dto.InvoiceId}";
            else if (dto.SubscriptionId.HasValue)
                orderInfo = $"Thanh toán subscription #{dto.SubscriptionId}";
            else
                throw new Exception("Phải có BookingId, InvoiceId hoặc SubscriptionId để tạo thanh toán.");

            dto.Description = orderInfo;
            return await _vnPay.CreatePaymentUrl(dto, ipAddress, txnRef);
        }

        // ===========================================================
        // 🔹 2️⃣ Tạo payment URL riêng cho Subscription
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
                SubscriptionId = subscriptionId,
                Description = orderInfo
            };

            return await _vnPay.CreatePaymentUrl(dto, ipAddress, txnRef);
        }

        // ===========================================================
        // 🔹 3️⃣ Tạo payment URL combo (Invoice + Subscription)
        // ===========================================================
        public async Task<string> CreateComboPaymentUrl(int invoiceId, int subscriptionId, string ipAddress)
        {
            var invoice = await _invoiceRepo.GetByIdAsync(invoiceId)
                ?? throw new Exception($"Không tìm thấy hóa đơn #{invoiceId}");

            var sub = await _subscriptionRepo.GetByIdAsync(subscriptionId)
                ?? throw new Exception($"Không tìm thấy Subscription #{subscriptionId}");

            var plan = await _planRepo.GetByIdAsync(sub.SubscriptionPlanId)
                ?? throw new Exception("Không tìm thấy gói Subscription.");

            decimal totalAmount = (invoice.Total ?? 0) + plan.PriceMonthly;

            var txnRef = Guid.NewGuid().ToString("N").Substring(0, 10);
            string orderInfo = $"Thanh toán combo Invoice#{invoiceId}_Sub#{subscriptionId}";

            // ⚡️ Tạo request DTO tạm để truyền qua VNPay service
            var dto = new PaymentCreateDto
            {
                InvoiceId = invoiceId,                // ✅ thêm
                SubscriptionId = subscriptionId,      // ✅ thêm
                Description = orderInfo,
            };


            return await _vnPay.CreatePaymentUrl(dto, ipAddress, txnRef);
        }

        // ===========================================================
        // 🔹 4️⃣ Xử lý callback (Booking / Invoice / Subscription / Combo)
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

            // 🧩 Nhận diện combo
            if (orderInfo.Contains("combo"))
            {
                var parts = orderInfo.Split('_');
                int.TryParse(parts[0].Split('#').LastOrDefault(), out int invoiceId);
                int.TryParse(parts[1].Split('#').LastOrDefault(), out int subId);
                return await HandleComboPaymentAsync(invoiceId, subId);
            }

            // ⚙️ Các loại khác
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
        // 🔹 5️⃣ Xử lý thanh toán Booking
        // ===========================================================
        private async Task<string> HandleBookingPaymentAsync(int bookingId)
        {
            var booking = await _bookingRepo.GetByIdAsync(bookingId)
                ?? throw new Exception($"Không tìm thấy Booking #{bookingId}.");

            var payment = new Payment
            {
                BookingId = booking.BookingId,
                CustomerId = booking.CustomerId ?? 0,
                CompanyId = booking.CompanyId,
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
            await _notiRepo.AddAsync(new Notification
            {
                CustomerId = booking.CustomerId,
                CompanyId = booking.CompanyId,
                BookingId = booking.BookingId,
                Title = "Thanh toán đặt lịch thành công",
                Message = $"Đặt lịch #{booking.BookingId} đã được thanh toán thành công.",
                Type = "Booking",
                Priority = "Normal",
                ActionUrl = $"/payment/success?bookingId={bookingId}"
            });

            return $"✅ Thanh toán thành công cho Booking #{booking.BookingId}.";
        }

        // ===========================================================
        // 🔹 6️⃣ Xử lý thanh toán Invoice
        // ===========================================================
        private async Task<string> HandleInvoicePaymentAsync(int invoiceId)
        {
            var invoice = await _invoiceRepo.GetByIdAsync(invoiceId)
                ?? throw new Exception($"Không tìm thấy Hóa đơn #{invoiceId}.");

            var now = DateTime.Now;

            // 🔹 Tạo payment record
            var payment = new Payment
            {
                InvoiceId = invoice.InvoiceId,
                CustomerId = invoice.CustomerId ?? 0,
                CompanyId = invoice.CompanyId,
                Amount = invoice.Total,
                Method = "VNPAY",
                Status = "Success",
                PaidAt = now,
                CreatedAt = now,
                UpdatedAt = now
            };

            // 🔹 Cập nhật invoice thành Paid
            invoice.Status = "Paid";
            invoice.UpdatedAt = now;
            await _invoiceRepo.UpdateAsync(invoice);
            await _paymentRepo.AddAsync(payment);
            await _invoiceRepo.SaveAsync();

            // ✅ Nếu hóa đơn này thuộc về subscription → kích hoạt gói đó
            if (invoice.SubscriptionId.HasValue)
            {
                var sub = await _subscriptionRepo.GetByIdAsync(invoice.SubscriptionId.Value);
                if (sub != null)
                {
                    // Lấy gói để biết thời hạn
                    var plan = await _planRepo.GetByIdAsync(sub.SubscriptionPlanId);
                    if (plan != null)
                    {
                        sub.Status = "Active";
                        if (sub.StartDate == default(DateTime))
                            sub.StartDate = now;
                        sub.EndDate = now.AddMonths(1);
                        sub.NextBillingDate = sub.EndDate;
                        sub.UpdatedAt = now;

                        await _subscriptionRepo.UpdateAsync(sub);

                        // 🔔 Gửi thông báo kích hoạt gói
                        await _notiRepo.AddAsync(new Notification
                        {
                            CustomerId = sub.CustomerId,
                            CompanyId = sub.CompanyId,
                            SubscriptionId = sub.SubscriptionId,
                            Title = "Kích hoạt gói đăng ký thành công",
                            Message = $"Gói {plan.PlanName} của bạn đã được kích hoạt và có hiệu lực đến {sub.EndDate:dd/MM/yyyy}.",
                            Type = "Subscription",
                            Priority = "High",
                            ActionUrl = $"/manageSubcription"
                        });
                    }
                }
            }

            // 🔔 Thông báo thanh toán hóa đơn
            await _notiRepo.AddAsync(new Notification
            {
                CustomerId = invoice.CustomerId,
                CompanyId = invoice.CompanyId,
                InvoiceId = invoice.InvoiceId,
                Title = "Thanh toán hóa đơn thành công",
                Message = $"Hóa đơn #{invoice.InvoiceId} đã được thanh toán thành công.",
                Type = "Invoice",
                Priority = "Normal",
                ActionUrl = $"/invoiceDetail/{invoiceId}"
            });

            return $"✅ Thanh toán thành công cho Hóa đơn #{invoice.InvoiceId}.";
        }


        // ===========================================================
        // 🔹 7️⃣ Xử lý thanh toán Subscription
        // ===========================================================
        private async Task<string> HandleSubscriptionPaymentAsync(int subscriptionId)
        {
            var sub = await _subscriptionRepo.GetByIdAsync(subscriptionId)
                ?? throw new Exception($"Không tìm thấy Subscription #{subscriptionId}.");

            var plan = await _planRepo.GetByIdAsync(sub.SubscriptionPlanId)
                ?? throw new Exception("Không tìm thấy gói Subscription.");

            decimal amount = plan.PriceMonthly;
            var now = DateTime.Now;

            // 🧾 Tạo bản ghi payment
            var payment = new Payment
            {
                SubscriptionId = sub.SubscriptionId,
                CustomerId = sub.CustomerId ?? 0,
                CompanyId = sub.CompanyId,
                Amount = amount,
                Method = "VNPAY",
                Status = "Success",
                PaidAt = now,
                CreatedAt = now,
                UpdatedAt = now
            };

            // 🔁 Gia hạn hoặc kích hoạt mới
            if (sub.Status != "Active" || sub.EndDate == null || sub.EndDate < now)
            {
                // 🔹 Lần đầu kích hoạt
                sub.StartDate = now;
                sub.EndDate = now.AddMonths(1);
                sub.NextBillingDate = sub.EndDate;
                sub.Status = "Active";
            }
            else
            {
                // 🔹 Gia hạn thêm 1 tháng
                sub.EndDate = sub.EndDate.Value.AddMonths(1);
                sub.NextBillingDate = sub.EndDate;
            }

            sub.UpdatedAt = now;
            await _subscriptionRepo.UpdateAsync(sub);

            await _paymentRepo.AddAsync(payment);
            await _paymentRepo.SaveAsync();

            // ✅ Nếu có invoice Unpaid của gói này → mark Paid
            var unpaidInvoices = await _invoiceRepo.GetAllAsync(i =>
                i.SubscriptionId == sub.SubscriptionId && i.Status == "Unpaid");

            if (unpaidInvoices.Any())
            {
                foreach (var inv in unpaidInvoices)
                {
                    inv.Status = "Paid";
                    inv.UpdatedAt = now;
                    await _invoiceRepo.UpdateAsync(inv);
                }
                await _invoiceRepo.SaveAsync();
            }

            // 🧾 Tạo hóa đơn mới (cho lần gia hạn hoặc chu kỳ kế tiếp)
            var newInvoice = new Invoice
            {
                CustomerId = sub.CustomerId,
                CompanyId = sub.CompanyId,
                SubscriptionId = sub.SubscriptionId,
                BillingMonth = now.Month,
                BillingYear = now.Year,
                Subtotal = amount,
                Tax = 0,                      // ❌ Không tính VAT cho Subscription
                Total = amount,               // ✅ Tổng = giá gói
                Status = "Paid",
                CreatedAt = now,
                UpdatedAt = now,
                IsMonthlyInvoice = false
            };

            await _invoiceRepo.AddAsync(newInvoice);

            // 🔔 Gửi thông báo
            await _notiRepo.AddAsync(new Notification
            {
                CustomerId = sub.CustomerId,
                CompanyId = sub.CompanyId,
                SubscriptionId = sub.SubscriptionId,
                Title = "Thanh toán gói đăng ký thành công",
                Message = $"Gói {plan.PlanName} của bạn đã được kích hoạt / gia hạn đến {sub.EndDate:dd/MM/yyyy}.",
                Type = "Subscription",
                Priority = "High",
                ActionUrl = $"/manageSubcription"
            });

            return $"✅ Thanh toán thành công Subscription #{sub.SubscriptionId}.";
        }


        // ===========================================================
        // 🔹 8️⃣ Xử lý thanh toán Combo (Invoice + Subscription)
        // ===========================================================
        private async Task<string> HandleComboPaymentAsync(int invoiceId, int subscriptionId)
        {
            var invoiceMsg = await HandleInvoicePaymentAsync(invoiceId);
            var subMsg = await HandleSubscriptionPaymentAsync(subscriptionId);

            var invoice = await _invoiceRepo.GetByIdAsync(invoiceId);
            var sub = await _subscriptionRepo.GetByIdAsync(subscriptionId);
            var plan = await _planRepo.GetByIdAsync(sub.SubscriptionPlanId);

            decimal total = (invoice.Total ?? 0) + plan.PriceMonthly;

            var comboPayment = new Payment
            {
                CustomerId = invoice.CustomerId ?? sub.CustomerId ?? 0,
                CompanyId = invoice.CompanyId ?? sub.CompanyId,
                Amount = total,
                Method = "VNPAY",
                Status = "Success",
                PaidAt = DateTime.Now,
                CreatedAt = DateTime.Now
            };
            await _paymentRepo.AddAsync(comboPayment);

            await _notiRepo.AddAsync(new Notification
            {
                CustomerId = invoice.CustomerId ?? sub.CustomerId,
                CompanyId = invoice.CompanyId ?? sub.CompanyId,
                Title = "Thanh toán combo thành công",
                Message = $"Bạn đã thanh toán thành công hóa đơn #{invoiceId} và gói đăng ký #{subscriptionId}.",
                Type = "Payment",
                Priority = "High",
                ActionUrl = $"/payments/{comboPayment.PaymentId}"
            });

            return $"✅ Thanh toán combo thành công!\n{invoiceMsg}\n{subMsg}";
        }
        // ===========================================================
        // 🔹 Thanh toán phiên sạc vãng lai (Guest Session)
        // ===========================================================
        public async Task<string> CreateGuestSessionPaymentUrl(int sessionId, string ipAddress)
        {
            var session = await _chargingSessionRepo.GetByIdAsync(sessionId)
                ?? throw new Exception($"Không tìm thấy phiên sạc #{sessionId}.");

            if (session.Total == null || session.Total <= 0)
                throw new Exception("Phiên sạc chưa có tổng tiền để thanh toán.");

            string txnRef = Guid.NewGuid().ToString("N").Substring(0, 10);
            string orderInfo = $"Thanh toán phiên sạc #{session.ChargingSessionId}";

            var dto = new PaymentCreateDto
            {
                ChargingSessionId = session.ChargingSessionId,
                Description = orderInfo
            };


            // ✅ Tạo URL VNPay
            return await _vnPay.CreatePaymentUrl(dto, ipAddress, txnRef);
        }
        public async Task<string> HandleGuestSessionPaymentAsync(int sessionId)
        {
            var session = await _chargingSessionRepo.GetByIdAsync(sessionId)
                ?? throw new Exception($"Không tìm thấy phiên sạc #{sessionId}.");

            var payment = new Payment
            {
                ChargingSessionId = session.ChargingSessionId,
                Amount = session.Total,
                Method = "VNPAY",
                Status = "Success",
                PaidAt = DateTime.Now,
                CreatedAt = DateTime.Now
            };

            await _paymentRepo.AddAsync(payment);
            session.Status = "Paid";
            await _chargingSessionRepo.UpdateAsync(session);

            return $"✅ Thanh toán thành công phiên sạc #{session.ChargingSessionId}.";
        }

    }
}
