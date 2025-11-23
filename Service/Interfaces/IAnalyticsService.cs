using Repositories.DTOs;

namespace Services.Interfaces
{
    public interface IAnalyticsService
    {
        Task<AnalyticsSummaryDto> GetSummaryAsync(int month, int year, int? companyId, bool adminView);
        Task<RevenueSourceDto> GetRevenueSourcesAsync(int month, int year, int? companyId, bool adminView);

        Task<List<BreakdownItemDto>> GetBreakdownByCompanyAsync(int month, int year);              // Admin only
        Task<List<VehicleBreakdownItemDto>> GetBreakdownByVehicleAsync(int month, int year, int companyId); // Company (self)
        Task<List<BreakdownItemDto>> GetBreakdownByStationAsync(int month, int year, int? companyId, bool adminView);
        Task<List<BreakdownItemDto>> GetBreakdownByChargerAsync(int month, int year, int? companyId, bool adminView);
        Task<List<BreakdownItemDto>> GetBreakdownByPortAsync(int month, int year, int? companyId, bool adminView);

        Task<List<BreakdownItemDto>> GetBreakdownByConnectorTypeAsync(int month, int year, int? companyId, bool adminView);
        Task<List<BreakdownItemDto>> GetBreakdownByVehicleTypeAsync(int month, int year, int? companyId, bool adminView);
        Task<List<BreakdownItemDto>> GetBreakdownByTimeRangeAsync(int month, int year, int? companyId, bool adminView);

        Task<List<HardwareUtilDto>> GetHardwareUtilizationAsync(
            int month, int year, int? companyId, bool adminView,
            string scope, double minUtilization = 0.05, int minSessions = 3);

        Task<TopListDto> GetTopAndUnderAsync(
            int month, int year, int? companyId, bool adminView,
            double minUtilization = 0.05, int minSessions = 3);
    }
}
