namespace Repositories.DTOs.PricingRules
{
    public class PricingRuleDetailDto
    {
        public int PricingRuleId { get; set; }
        public string ChargerType { get; set; }
        public decimal PowerKw { get; set; }
        public string TimeRange { get; set; }
        public decimal PricePerKwh { get; set; }
        public decimal IdleFeePerMin { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
