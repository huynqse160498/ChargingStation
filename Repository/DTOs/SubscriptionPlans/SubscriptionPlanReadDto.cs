using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.DTOs.SubscriptionPlans
{
    public class SubscriptionPlanReadDto
    {
        public int SubscriptionPlanId { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = "Individual";
        public decimal PriceMonthly { get; set; }
        public decimal? PriceYearly { get; set; }
        public decimal? DiscountPercent { get; set; }
        public int? FreeIdleMinutes { get; set; }
        public string Benefits { get; set; } = string.Empty;
        public bool IsForCompany { get; set; }
        public string Status { get; set; } = "Active";
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
