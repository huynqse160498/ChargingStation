using Repositories.DTOs.StationStaff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IStationStaffService
    {
        // Danh sách staff theo station
        Task<List<StationStaffReadDto>> ListByStationAsync(int stationId);

        // Thêm gán 1 staff vào station
        Task<StationStaffReadDto> AddAsync(StationStaffCreateDto dto);

        // Gỡ staff khỏi station
        Task<bool> DeleteAsync(int stationId, int staffId);
    }
}
