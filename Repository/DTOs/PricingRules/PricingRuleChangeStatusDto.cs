using System.ComponentModel.DataAnnotations;

namespace Repositories.DTOs.PricingRules
{
    public class PricingRuleChangeStatusDto
    {
        [Required(ErrorMessage = "Trạng thái không được để trống")]
        [RegularExpression("^(Active|Inactive)$", ErrorMessage = "Trạng thái chỉ có thể là Active hoặc Inactive")]
        public string Status { get; set; }
    }
}
