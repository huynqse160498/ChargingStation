using Repositories.DTOs.Reports;
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
    public class ReportService : IReportService
    {
        private readonly IReportRepository _repo;
        private readonly IStationRepository _stationRepo;
        private readonly IChargerRepository _chargerRepo;
        private readonly IPortRepository _portRepo;
        private readonly IAccountRepository _accountRepo;

        private static readonly HashSet<string> AllowedSeverities =
            new(StringComparer.OrdinalIgnoreCase) { "Low", "Medium", "High", "Critical" };

        private static readonly HashSet<string> AllowedStatuses =
            new(StringComparer.OrdinalIgnoreCase) { "Pending", "InProgress", "Resolved", "Closed" };

        public ReportService(
            IReportRepository repo,
            IStationRepository stationRepo,
            IChargerRepository chargerRepo,
            IPortRepository portRepo,
            IAccountRepository accountRepo)
        {
            _repo = repo;
            _stationRepo = stationRepo;
            _chargerRepo = chargerRepo;
            _portRepo = portRepo;
            _accountRepo = accountRepo;
        }

        // ========================= Queries =========================
        public async Task<ReportReadDto> GetAsync(int id)
        {
            var e = await _repo.GetByIdAsync(id) ?? throw new KeyNotFoundException("Report không tồn tại.");
            return Map(e);
        }

        public async Task<List<ReportReadDto>> ListByStationAsync(int stationId)
        {
            if (stationId <= 0) return new();
            var list = await _repo.GetByStationAsync(stationId);
            return list.Select(Map).ToList();
        }

        public async Task<(int total, List<ReportReadDto> items)> GetPagedAsync(
            int page, int pageSize,
            int? stationId, int? chargerId, string? status, string? severity,
            DateTime? from, DateTime? to)
        {
            var total = await _repo.CountAsync(stationId, chargerId, status, severity, from, to);
            var data = await _repo.GetPagedAsync(page, pageSize, stationId, chargerId, status, severity, from, to);
            return (total, data.Select(Map).ToList());
        }

        // ========================= Commands =========================
        public async Task<ReportReadDto> CreateAsync(ReportCreateDto dto)
        {
            // Validate foreign keys
            _ = await _accountRepo.GetByIdAsync(dto.StaffId) ?? throw new KeyNotFoundException("Staff không tồn tại.");
            var st = await _stationRepo.GetByIdAsync(dto.StationId) ?? throw new KeyNotFoundException("Station không tồn tại.");

            if (dto.ChargerId is int cid)
            {
                var ch = await _chargerRepo.GetByIdAsync(cid) ?? throw new KeyNotFoundException("Charger không tồn tại.");
                // nếu có cả StationId + ChargerId thì bảo đảm cùng trạm
                if (ch.StationId != dto.StationId) throw new InvalidOperationException("Charger không thuộc station.");
            }
            if (dto.PortId is int pid)
            {
                var p = await _portRepo.GetByIdAsync(pid) ?? throw new KeyNotFoundException("Port không tồn tại.");
                if (dto.ChargerId is int cid2 && p.ChargerId != cid2) throw new InvalidOperationException("Port không thuộc charger.");
            }

            // Normalize enums
            var sev = string.IsNullOrWhiteSpace(dto.Severity) ? "Low" : dto.Severity!;
            if (!AllowedSeverities.Contains(sev)) throw new ArgumentException("Severity không hợp lệ (Low/Medium/High/Critical).");

            var status = string.IsNullOrWhiteSpace(dto.Status) ? "Pending" : dto.Status!;
            if (!AllowedStatuses.Contains(status)) throw new ArgumentException("Status không hợp lệ (Pending/InProgress/Resolved/Closed).");

            var e = new Report
            {
                StaffId = dto.StaffId,
                StationId = dto.StationId,
                ChargerId = dto.ChargerId,
                PortId = dto.PortId,
                Title = dto.Title,
                Description = dto.Description,
                Severity = sev,
                Status = status
            };

            await _repo.AddAsync(e);

            // đọc lại để có navigation cho mapping
            var saved = await _repo.GetByIdAsync(e.ReportId) ?? e;
            return Map(saved);
        }

        public async Task<ReportReadDto> UpdateAsync(int id, ReportUpdateDto dto)
        {
            var e = await _repo.GetByIdAsync(id) ?? throw new KeyNotFoundException("Report không tồn tại.");

            if (dto.Title != null) e.Title = dto.Title;
            if (dto.Description != null) e.Description = dto.Description;

            if (dto.Severity != null)
            {
                if (!AllowedSeverities.Contains(dto.Severity)) throw new ArgumentException("Severity không hợp lệ.");
                e.Severity = dto.Severity;
            }

            if (dto.Status != null)
            {
                if (!AllowedStatuses.Contains(dto.Status)) throw new ArgumentException("Status không hợp lệ.");
                e.Status = dto.Status;
            }

            if (dto.ResolvedAt.HasValue) e.ResolvedAt = dto.ResolvedAt;

            await _repo.UpdateAsync(e);
            var saved = await _repo.GetByIdAsync(id) ?? e;
            return Map(saved);
        }

        public Task<bool> DeleteAsync(int id) => _repo.DeleteAsync(id);

        // ========================= Mapping =========================
        private static ReportReadDto Map(Report e) => new()
        {
            ReportId = e.ReportId,
            StaffId = e.StaffId,
            StationId = e.StationId,
            ChargerId = e.ChargerId,
            PortId = e.PortId,
            Title = e.Title ?? string.Empty,
            Description = e.Description ?? string.Empty,
            Severity = e.Severity ?? "Low",
            Status = e.Status ?? "Pending",
            CreatedAt = e.CreatedAt,
            ResolvedAt = e.ResolvedAt,

            // Account trong project không có FullName/Email -> dùng UserName
            StaffName = e.Staff?.UserName,
            StationName = e.Station?.StationName,
            ChargerCode = e.Charger?.Code,
            PortCode = e.Port?.Code
        };
    }
}
