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
        Task<IEnumerable<PortReadDto>> GetAllAsync();
        Task<PortReadDto> GetByIdAsync(int id);
        Task<PortReadDto> CreateAsync(PortCreateDto dto);
        Task<bool> UpdateAsync(int id, PortUpdateDto dto);
        Task<bool> DeleteAsync(int id);

        // NEW
        Task<(IEnumerable<PortReadDto> Items, int Total)> GetPagedAsync(int page, int pageSize, int? chargerId, string? status);
        Task<bool> ChangeStatusAsync(int id, string status);
    }
}
