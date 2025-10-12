using Repositories.DTOs.Stations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IStationService
    {
        Task<IEnumerable<StationReadDto>> GetAllAsync();
        Task<StationReadDto> GetByIdAsync(int id);
        Task<StationReadDto> CreateAsync(StationCreateDto dto);
        Task<bool> UpdateAsync(int id, StationUpdateDto dto);
        Task<bool> DeleteAsync(int id);

        // NEW
        Task<(IEnumerable<StationReadDto> Items, int Total)> GetPagedAsync(
            int page, int pageSize, string? stationName, string? city, string? status);
        Task<bool> ChangeStatusAsync(int id, string status);
    }
}
