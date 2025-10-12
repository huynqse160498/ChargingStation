namespace Repositories.DTOs.PricingRules
{
    public class PricingRuleQueryDto
    {
        // Bộ lọc
        public string? VehicleType { get; set; }
        public string? TimeRange { get; set; }
        public string? Status { get; set; }
        public string? Search { get; set; }

        // Phân trang
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        // Sắp xếp
        public string? SortBy { get; set; } = "CreatedAt";
        public string? SortDir { get; set; } = "desc";
    }
}
