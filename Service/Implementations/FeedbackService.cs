using Repositories.DTOs.Feedbacks;
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
    public class FeedbackService : IFeedbackService
    {
        private readonly IFeedbackRepository _repo;
        private readonly ICustomerRepository _customerRepo;
        private readonly IStationRepository _stationRepo;
        private readonly IChargerRepository _chargerRepo;
        private readonly IPortRepository _portRepo;

        public FeedbackService(
            IFeedbackRepository repo,
            ICustomerRepository customerRepo,
            IStationRepository stationRepo,
            IChargerRepository chargerRepo,
            IPortRepository portRepo)
        {
            _repo = repo;
            _customerRepo = customerRepo;
            _stationRepo = stationRepo;
            _chargerRepo = chargerRepo;
            _portRepo = portRepo;
        }

        public async Task<FeedbackReadDto> GetAsync(int id)
        {
            var e = await _repo.GetByIdAsync(id) ?? throw new KeyNotFoundException("Feedback không tồn tại.");
            return Map(e);
        }

        public async Task<List<FeedbackReadDto>> ListByStationAsync(int stationId)
        {
            if (stationId <= 0) return new();
            var list = await _repo.GetByStationAsync(stationId);
            return list.Select(Map).ToList();
        }

        public async Task<(int total, List<FeedbackReadDto> items)> GetPagedAsync(int page, int pageSize, int? stationId, int? customerId, int? rating)
        {
            var total = await _repo.CountAsync(stationId, customerId, rating);
            var items = await _repo.GetPagedAsync(page, pageSize, stationId, customerId, rating);
            return (total, items.Select(Map).ToList());
        }

        public async Task<FeedbackReadDto> CreateAsync(FeedbackCreateDto dto)
        {
            // Validate
            _ = await _customerRepo.GetByIdAsync(dto.CustomerId) ?? throw new KeyNotFoundException("Customer không tồn tại.");
            if (dto.StationId is int sid) _ = await _stationRepo.GetByIdAsync(sid) ?? throw new KeyNotFoundException("Station không tồn tại.");
            if (dto.ChargerId is int cid) _ = await _chargerRepo.GetByIdAsync(cid) ?? throw new KeyNotFoundException("Charger không tồn tại.");
            if (dto.PortId is int pid) _ = await _portRepo.GetByIdAsync(pid) ?? throw new KeyNotFoundException("Port không tồn tại.");
            if (dto.Rating < 1 || dto.Rating > 5) throw new ArgumentOutOfRangeException(nameof(dto.Rating), "Rating phải từ 1..5.");

            var e = new Feedback
            {
                CustomerId = dto.CustomerId,
                StationId = dto.StationId,
                ChargerId = dto.ChargerId,
                PortId = dto.PortId,
                Rating = dto.Rating,
                Comment = dto.Comment
            };
            await _repo.AddAsync(e);

            // read back for navigation mapping
            var saved = await _repo.GetByIdAsync(e.FeedbackId) ?? e;
            return Map(saved);
        }

        public async Task<FeedbackReadDto> UpdateAsync(int id, FeedbackUpdateDto dto)
        {
            var e = await _repo.GetByIdAsync(id) ?? throw new KeyNotFoundException("Feedback không tồn tại.");

            if (dto.Rating.HasValue)
            {
                if (dto.Rating.Value < 1 || dto.Rating.Value > 5) throw new ArgumentOutOfRangeException(nameof(dto.Rating), "Rating phải từ 1..5.");
                e.Rating = dto.Rating.Value;
            }
            if (dto.Comment != null) e.Comment = dto.Comment;

            await _repo.UpdateAsync(e);
            var saved = await _repo.GetByIdAsync(id) ?? e;
            return Map(saved);
        }

        public Task<bool> DeleteAsync(int id) => _repo.DeleteAsync(id);

        // ------- mapping helper -------
        private static FeedbackReadDto Map(Feedback e) => new()
        {
            FeedbackId = e.FeedbackId,
            CustomerId = e.CustomerId,
            StationId = e.StationId,
            ChargerId = e.ChargerId,
            PortId = e.PortId,
            Rating = e.Rating,
            Comment = e.Comment ?? string.Empty,
            CreatedAt = e.CreatedAt,
            CustomerName = e.Customer?.FullName ?? e.Customer?.Email ?? e.CustomerId.ToString(),
            StationName = e.Station?.StationName,
            ChargerCode = e.Charger?.Code,
            PortCode = e.Port?.Code
        };
    }
}
