using Repositories.Models;

public class Subscription
{
    public int SubscriptionId { get; set; }
    public int SubscriptionPlanId { get; set; }
    public int CustomerId { get; set; } // bắt buộc 1 trong Customer / Company
    public int? CompanyId { get; set; }

    public DateTime StartDate { get; set; } = DateTime.Now;
    public DateTime? EndDate { get; set; }
    public string BillingCycle { get; set; } = "Monthly"; // 'Monthly' | 'Yearly'
    public bool AutoRenew { get; set; } = true;
    public DateTime? NextBillingDate { get; set; }
    public string Status { get; set; } = "Active";

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    // 🔗 Navigation
    public virtual SubscriptionPlan SubscriptionPlan { get; set; }
    public virtual Customer Customer { get; set; }
    public virtual Company Company { get; set; }
    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}
