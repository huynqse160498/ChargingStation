using System.ComponentModel.DataAnnotations;

namespace Repositories.DTOs.PricingRules
{
    public class PricingRuleCreateDto
    {
        [Required(ErrorMessage = "Loại trụ không được để trống")]
        public string ChargerType { get; set; } // "AC" | "DC"

        [Range(0, double.MaxValue, ErrorMessage = "Công suất phải lớn hơn 0")]
        public decimal PowerKw { get; set; } // ví dụ: 60 hoặc 120

        [Required(ErrorMessage = "Khung giờ không được để trống")]
        public string TimeRange { get; set; } // "Low" | "Normal" | "Peak"

        [Range(0, double.MaxValue, ErrorMessage = "Giá sạc phải lớn hơn 0")]
        public decimal PricePerKwh { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Phí chờ phải lớn hơn 0")]
        public decimal IdleFeePerMin { get; set; }

        [Required(ErrorMessage = "Trạng thái không được để trống")]
        [RegularExpression("^(Active|Inactive)$", ErrorMessage = "Trạng thái chỉ có thể là Active hoặc Inactive")]
        public string Status { get; set; } = "Active";
    }
}
