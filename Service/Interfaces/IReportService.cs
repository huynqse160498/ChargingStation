using Repositories.DTOs.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IReportService
    {
        Task<ReportReadDto> GetAsync(int id);
        Task<List<ReportReadDto>> ListByStationAsync(int stationId);

        Task<(int total, List<ReportReadDto> items)> GetPagedAsync(
            int page, int pageSize,
            int? stationId, int? chargerId, string? status, string? severity,
            DateTime? from, DateTime? to);

        Task<ReportReadDto> CreateAsync(ReportCreateDto dto);
        Task<ReportReadDto> UpdateAsync(int id, ReportUpdateDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
