using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface IStationStaffRepository
    {
        // Lấy theo station
        Task<List<StationStaff>> GetByStationAsync(int stationId);

        // Lấy 1 bản ghi theo khóa kép
        Task<StationStaff?> GetAsync(int stationId, int staffId);

        // Thêm gán nhân viên vào station
        Task AddAsync(StationStaff entity);

        // Xóa gán nhân viên khỏi station
        Task<bool> DeleteAsync(int stationId, int staffId);

        // Kiểm tra tồn tại
        Task<bool> ExistsAsync(int stationId, int staffId);

        

    }
}
