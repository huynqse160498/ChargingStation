using System.ComponentModel.DataAnnotations;

namespace Repositories.DTOs.PricingRules
{
    public class PricingRuleUpdateDto
    {
        [Required]
        public int PricingRuleId { get; set; }

        [Required]
        public string VehicleType { get; set; }

        [Required]
        public string TimeRange { get; set; }

        [Range(0, double.MaxValue)]
        public decimal PricePerKwh { get; set; }

        [Range(0, double.MaxValue)]
        public decimal IdleFeePerMin { get; set; }

        [Required]
        public string Status { get; set; }
    }
}
