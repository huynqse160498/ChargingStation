using Microsoft.AspNetCore.Http;
using Repositories.DTOs.Ports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IPortService
    {
        // Lấy toàn bộ danh sách Port (admin/test)
        Task<IEnumerable<PortReadDto>> GetAllAsync();

        // Lấy 1 Port theo Id
        Task<PortReadDto> GetByIdAsync(int id);

        // Thêm mới 1 Port
        Task<PortReadDto> CreateAsync(PortCreateDto dto);

        // Cập nhật thông tin Port
        Task<bool> UpdateAsync(int id, PortUpdateDto dto);

        // Xoá 1 Port
        Task<bool> DeleteAsync(int id);

        // NEW: phân trang + filter theo ChargerId, Status
        Task<(IEnumerable<PortReadDto> Items, int Total)> GetPagedAsync(
            int page, int pageSize, int? chargerId, string? status);

        // NEW: đổi trạng thái nhanh (Available / Reserved / Occupied / Disabled)
        Task<bool> ChangeStatusAsync(int id, string status);

        // ======================= [IMAGE UPLOAD] =======================
        Task<PortReadDto> UploadImageAsync(int id, IFormFile file);
    }
}