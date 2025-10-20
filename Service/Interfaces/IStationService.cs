using Repositories.DTOs.Stations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Services.Interfaces
{
    public interface IStationService
    {
        // Lấy toàn bộ danh sách trạm (admin/test)
        Task<IEnumerable<StationReadDto>> GetAllAsync();

        // Lấy trạm theo Id
        Task<StationReadDto> GetByIdAsync(int id);

        // Tạo mới trạm (mặc định Status = "Open")
        Task<StationReadDto> CreateAsync(StationCreateDto dto);

        // Cập nhật thông tin trạm
        Task<bool> UpdateAsync(int id, StationUpdateDto dto);

        // Xoá trạm
        Task<bool> DeleteAsync(int id);

        // NEW : phân trang + filter theo StationName, City, Status
        Task<(IEnumerable<StationReadDto> Items, int Total)> GetPagedAsync(
            int page, int pageSize, string? stationName, string? city, string? status);

        // NEW: đổi trạng thái nhanh (Open / Closed)
        Task<bool> ChangeStatusAsync(int id, string status);


        // ======================= [IMAGE UPLOAD] =======================
        Task<StationReadDto> UploadImageAsync(int id, IFormFile file);
    }
}
