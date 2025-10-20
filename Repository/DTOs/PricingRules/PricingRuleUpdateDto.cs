using System.ComponentModel.DataAnnotations;

namespace Repositories.DTOs.PricingRules
{
    public class PricingRuleUpdateDto
    {
        [Required]
        public int PricingRuleId { get; set; }

        [Required(ErrorMessage = "Loại trụ không được để trống")]
        public string ChargerType { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Công suất phải lớn hơn 0")]
        public decimal PowerKw { get; set; }

        [Required(ErrorMessage = "Khung giờ không được để trống")]
        public string TimeRange { get; set; }

        [Range(0, double.MaxValue)]
        public decimal PricePerKwh { get; set; }

        [Range(0, double.MaxValue)]
        public decimal IdleFeePerMin { get; set; }

        [Required]
        [RegularExpression("^(Active|Inactive)$", ErrorMessage = "Trạng thái chỉ có thể là Active hoặc Inactive")]
        public string Status { get; set; }
    }
}
