using System;

namespace Repositories.Models
{
    public partial class Notification
    {
        public int NotificationId { get; set; }

        // 🔹 Người nhận
        public int? CustomerId { get; set; }
        public int? CompanyId { get; set; }

        // 🔹 Liên kết nghiệp vụ (nếu có)
        public int? BookingId { get; set; }
        public int? InvoiceId { get; set; }
        public int? SubscriptionId { get; set; }

        // 🔹 Nội dung
        public string Title { get; set; }
        public string Message { get; set; }
        //
        public string Type { get; set; } = "System"; // System / Manual / Alert / InvoiceReminder / Policy
        public string Priority { get; set; } = "Normal"; // Low / Normal / High
         //                                           
        public string? ActionUrl { get; set; } // VD: /invoice/5, /subscription/detail/3

        // 🔹 Gửi bởi ai (Admin)
        public int? SenderAdminId { get; set; }

        // 🔹 Trạng thái
        public bool IsRead { get; set; } = false;
        public bool IsArchived { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // 🔗 Navigation
        public virtual Customer Customer { get; set; }
        public virtual Company Company { get; set; }
        public virtual Booking Booking { get; set; }
        public virtual Invoice Invoice { get; set; }
        public virtual Subscription Subscription { get; set; }
    }
}
