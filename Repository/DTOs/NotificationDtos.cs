namespace Repositories.DTOs.Notifications
{
    public class NotificationCreateDto
    {
        public int? CustomerId { get; set; }
        public int? CompanyId { get; set; }

        public int? BookingId { get; set; }
        public int? InvoiceId { get; set; }
        public int? SubscriptionId { get; set; }

        public string Title { get; set; }
        public string Message { get; set; }
        public string Type { get; set; } = "System";   // System / Manual / Invoice / Booking / Subscription / Alert
        public string Priority { get; set; } = "Normal"; // Low / Normal / High
        public int? SenderAdminId { get; set; }
    }

    public class NotificationReadDto
    {
        public int NotificationId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
        public string Priority { get; set; }
        public string? ActionUrl { get; set; }
        public bool IsRead { get; set; }
        public bool IsArchived { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // ✅ Dùng riêng cho “admin gửi”
    public class AdminSendToCustomerDto
    {
        public int CustomerId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Type { get; set; } = "Manual";
        public string Priority { get; set; } = "Normal";
        public int? SenderAdminId { get; set; }

        // Liên kết nghiệp vụ (nếu có)
        public int? BookingId { get; set; }
        public int? InvoiceId { get; set; }
        public int? SubscriptionId { get; set; }
    }

    public class AdminSendToCompanyDto
    {
        public int CompanyId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Type { get; set; } = "Manual";
        public string Priority { get; set; } = "Normal";
        public int? SenderAdminId { get; set; }

        public int? BookingId { get; set; }
        public int? InvoiceId { get; set; }
        public int? SubscriptionId { get; set; }
    }

    public class AdminBroadcastDto
    {
        public string Audience { get; set; } = "All"; // All | Customers | Companies
        public string Title { get; set; }
        public string Message { get; set; }
        public string Type { get; set; } = "Manual";
        public string Priority { get; set; } = "Normal";
        public int? SenderAdminId { get; set; }

        // Optionally attach a business link for everyone
        public int? BookingId { get; set; }
        public int? InvoiceId { get; set; }
        public int? SubscriptionId { get; set; }
    }
}
