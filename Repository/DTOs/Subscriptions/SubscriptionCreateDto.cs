using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.DTOs.Subscriptions
{
    public class SubscriptionCreateDto
    {
        [Required] public int SubscriptionPlanId { get; set; }
        public int? CustomerId { get; set; }
        public int? CompanyId { get; set; }

        [Required] public string BillingCycle { get; set; } = "Monthly"; 
        public bool AutoRenew { get; set; } = true;

        public DateTime StartDate { get; set; } = DateTime.Now;
        public string Status { get; set; } = "Active";
    }
}
