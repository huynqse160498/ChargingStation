namespace Repositories.DTOs
{
    // ✅ Dùng để tạo hóa đơn thủ công (admin)
    public class InvoiceCreateDto
    {
        public int CustomerId { get; set; }
        public int? SubscriptionId { get; set; }
        public int BillingMonth { get; set; }
        public int BillingYear { get; set; }
    }

    // ✅ Cập nhật trạng thái hóa đơn
    public class InvoiceUpdateStatusDto
    {
        public int InvoiceId { get; set; }
        public string Status { get; set; } // "Paid" | "Overdue" | "Unpaid"
    }

    // ✅ Dùng để trả kết quả chi tiết
    public class InvoiceDto
    {
        public int InvoiceId { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public int? SubscriptionId { get; set; }
        public decimal? Subtotal { get; set; }
        public decimal? Tax { get; set; }
        public decimal? Total { get; set; }
        public int BillingMonth { get; set; }
        public int BillingYear { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> ChargingSessions { get; set; }
    }
}
