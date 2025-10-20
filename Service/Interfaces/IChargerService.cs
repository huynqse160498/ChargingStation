using Microsoft.AspNetCore.Http;
using Repositories.DTOs.Chargers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Services.Interfaces
{
    public interface IChargerService
    {
        Task<IEnumerable<ChargerReadDto>> GetAllAsync();

        Task<ChargerReadDto> GetByIdAsync(int id);

        Task<ChargerReadDto> CreateAsync(ChargerCreateDto dto);

        Task<bool> UpdateAsync(int id, ChargerUpdateDto dto);

        Task<bool> DeleteAsync(int id);
    

        Task<(IEnumerable<ChargerReadDto> Items, int Total)> GetPagedAsync(
            int page, int pageSize,
            int? stationId, string? code, string? type, string? status,
            decimal? minPower, decimal? maxPower);

        Task<bool> ChangeStatusAsync(int id, string status);

        // ======================= [IMAGE UPLOAD] =======================
        Task<ChargerReadDto> UploadImageAsync(int id, IFormFile file); // NEW
    }
}
