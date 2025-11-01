using Repositories.DTOs.StationStaff;
using Repositories.Interfaces;
using Repositories.Models;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implementations
{
    public class StationStaffService : IStationStaffService
    {
        private readonly IStationStaffRepository _repo;
        private readonly IStationRepository _stationRepo;
        private readonly IAccountRepository _accountRepo;

        public StationStaffService(
            IStationStaffRepository repo,
            IStationRepository stationRepo,
            IAccountRepository accountRepo)
        {
            _repo = repo;
            _stationRepo = stationRepo;
            _accountRepo = accountRepo;
        }

        public async Task<List<StationStaffReadDto>> ListByStationAsync(int stationId)
        {
            if (stationId <= 0) return new List<StationStaffReadDto>();

            var list = await _repo.GetByStationAsync(stationId);
            return list.Select(MapToRead).ToList();
        }

        public async Task<StationStaffReadDto> AddAsync(StationStaffCreateDto dto)
        {
            // Validate inputs
            var station = await _stationRepo.GetByIdAsync(dto.StationId)
                ?? throw new KeyNotFoundException("Station không tồn tại.");
            var staff = await _accountRepo.GetByIdAsync(dto.StaffId)
                ?? throw new KeyNotFoundException("Nhân viên không tồn tại.");

            // Check duplicate
            if (await _repo.ExistsAsync(dto.StationId, dto.StaffId))
                throw new InvalidOperationException("Nhân viên đã thuộc station này.");

            var entity = new StationStaff
            {
                StationId = dto.StationId,
                StaffId = dto.StaffId
            };

            await _repo.AddAsync(entity);

            // Gắn staff để map ra dto
            entity.Staff = staff;
            return MapToRead(entity);
        }

        public Task<bool> DeleteAsync(int stationId, int staffId)
            => _repo.DeleteAsync(stationId, staffId);

        // =============== helpers ===============
        private static StationStaffReadDto MapToRead(StationStaff e) => new()
        {
            StationId = e.StationId,
            StaffId = e.StaffId,
            StaffName = e.Staff?.UserName ?? string.Empty,
            StaffEmail = string.Empty
        };
    }
}
