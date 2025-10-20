using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.DTOs.Subscriptions
{
    public class SubscriptionReadDto
    {
        public int SubscriptionId { get; set; }
        public int SubscriptionPlanId { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public int? CustomerId { get; set; }
        public int? CompanyId { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string BillingCycle { get; set; } = string.Empty;
        public bool AutoRenew { get; set; }
        public DateTime? NextBillingDate { get; set; }
        public string Status { get; set; } = "Active";
    }
}
