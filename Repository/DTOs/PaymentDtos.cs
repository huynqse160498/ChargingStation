using System.ComponentModel.DataAnnotations;

namespace Repositories.DTOs
{
    // 🧾 DTO dùng khi tạo thanh toán
    public class PaymentCreateDto
    {
        [Required(ErrorMessage = "Vui lòng nhập mã đặt lịch.")]
        public int BookingId { get; set; }

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
