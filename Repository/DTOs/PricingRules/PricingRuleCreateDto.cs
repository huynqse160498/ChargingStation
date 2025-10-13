using System.ComponentModel.DataAnnotations;

namespace Repositories.DTOs.PricingRules
{
    public class PricingRuleCreateDto
    {
        [Required(ErrorMessage = "Loại xe không được để trống")]
        public string VehicleType { get; set; } // "Car" | "Motorbike"

        [Required(ErrorMessage = "Khung giờ không được để trống")]
        public string TimeRange { get; set; } // "Low" | "Normal" | "Peak"

        [Range(0, double.MaxValue, ErrorMessage = "Giá sạc phải lớn hơn 0")]
        public decimal PricePerKwh { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Phí chờ phải lớn hơn 0")]
        public decimal IdleFeePerMin { get; set; }

        [Required(ErrorMessage = "Trạng thái không được để trống")]
        public string Status { get; set; } // "Active" | "Inactive"
    }
}
