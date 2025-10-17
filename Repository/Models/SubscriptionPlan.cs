using System;
using System.Collections.Generic;

namespace Repositories.Models
{
    public class SubscriptionPlan
    {
        public int SubscriptionPlanId { get; set; }
        public string PlanName { get; set; }
        public string Description { get; set; }
        public string Category { get; set; } // "Individual" hoặc "Business"
        public decimal PriceMonthly { get; set; }
        public decimal? PriceYearly { get; set; }
        public decimal? DiscountPercent { get; set; }
        public int? FreeIdleMinutes { get; set; }
        public string Benefits { get; set; }
        public bool IsForCompany { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public string Status { get; set; } = "Active";

        // Navigation
        public virtual ICollection<Subscription> Subscriptions { get; set; }
    }
}
