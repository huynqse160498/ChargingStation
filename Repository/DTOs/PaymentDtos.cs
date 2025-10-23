using System.ComponentModel.DataAnnotations;

namespace Repositories.DTOs
{
    // 🧾 DTO dùng khi tạo thanh toán
    public class PaymentCreateDto
    {
        public int? BookingId { get; set; }
        public int? InvoiceId { get; set; } // 🔗 Hóa đơn tháng
        public int? CompanyId { get; set; }  // nếu công ty thanh toán
        public int? SubscriptionId { get; set; } // ✅ đúng chính tả, trùng với model
        public string? Description { get; set; }
    }

    public class PaymentListItemDto
    {
        public int PaymentId { get; set; }
        public int BookingId { get; set; }
        public decimal? Amount { get; set; }
        public string Method { get; set; }
        public string Status { get; set; }
        public DateTime? PaidAt { get; set; }
    }

    public class PaymentDetailDto : PaymentListItemDto
    {
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
