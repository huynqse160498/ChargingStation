using System;
using System.Collections.Generic;

namespace Repositories.DTOs
{
    // ✅ Dùng để tạo hóa đơn thủ công (Admin)
    public class InvoiceCreateDto
    {
        public int? CustomerId { get; set; }         // Hóa đơn cho khách hàng cá nhân
        public int? CompanyId { get; set; }          // Hoặc hóa đơn dành cho công ty
        public int? SubscriptionId { get; set; }     // Gói đăng ký nếu có
        public int BillingMonth { get; set; }
        public int BillingYear { get; set; }

        public decimal Subtotal { get; set; }        // Tổng tiền trước thuế
        public decimal? Tax { get; set; }            // Thuế (mặc định 10%)
        public decimal? Total { get; set; }          // Tổng cuối cùng
        public string? Notes { get; set; }           // Ghi chú tùy chọn
    }

    // ✅ Cập nhật trạng thái hóa đơn
    public class InvoiceUpdateStatusDto
    {
        public int InvoiceId { get; set; }
        public string Status { get; set; } = "Unpaid"; // Paid | Overdue | Unpaid
    }

    // ✅ Dùng để trả về chi tiết hóa đơn (Admin / Customer)
    public class InvoiceDto
    {
        public int InvoiceId { get; set; }
        public int? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public int? CompanyId { get; set; }
        public string? CompanyName { get; set; }

        public int? SubscriptionId { get; set; }
        public decimal? Subtotal { get; set; }
        public decimal? SubscriptionAdjustment { get; set; }
        public decimal? Tax { get; set; }
        public decimal? Total { get; set; }

        public int BillingMonth { get; set; }
        public int BillingYear { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<InvoiceSessionDto> ChargingSessions { get; set; } = new();
    }

    // ✅ Thông tin rút gọn của các phiên sạc trong hóa đơn
    public class InvoiceSessionDto
    {
        public int SessionId { get; set; }
        public string PortName { get; set; }
        public decimal? Total { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
    }

    // ✅ Dùng cho danh sách hiển thị tổng quát (Admin dashboard)
    public class InvoiceListItemDto
    {
        public int InvoiceId { get; set; }
        public string CustomerOrCompany { get; set; }
        public decimal? Total { get; set; }
        public string Status { get; set; }
        public string Period => $"{BillingMonth:D2}/{BillingYear}";
        public int BillingMonth { get; set; }
        public int BillingYear { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
