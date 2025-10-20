using Microsoft.EntityFrameworkCore;
using Repositories.DTOs;
using Repositories.DTOs.PricingRules;
using Repositories.Interfaces;
using Repositories.Models;
using Services.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Services.Implementations
{
    public class PricingRuleService : IPricingRuleService
    {
        private readonly IPricingRuleRepository _repo;

        public PricingRuleService(IPricingRuleRepository repo)
        {
            _repo = repo;
        }

        // 🟩 Lấy danh sách (phân trang + tìm kiếm)
        public async Task<PagedResult<PricingRuleListItemDto>> GetAllAsync(PricingRuleQueryDto q)
        {
            var query = _repo.GetAll();

            if (!string.IsNullOrEmpty(q.ChargerType))
                query = query.Where(x => x.ChargerType == q.ChargerType);

            if (q.PowerKw.HasValue)
                query = query.Where(x => x.PowerKw == q.PowerKw.Value);

            if (!string.IsNullOrEmpty(q.TimeRange))
                query = query.Where(x => x.TimeRange == q.TimeRange);

            if (!string.IsNullOrEmpty(q.Status))
                query = query.Where(x => x.Status == q.Status);

            if (!string.IsNullOrEmpty(q.Search))
                query = query.Where(x => x.ChargerType.Contains(q.Search) || x.TimeRange.Contains(q.Search));

            bool desc = q.SortDir?.ToLower() == "desc";
            query = (q.SortBy ?? "CreatedAt").ToLower() switch
            {
                "price" => desc ? query.OrderByDescending(x => x.PricePerKwh) : query.OrderBy(x => x.PricePerKwh),
                _ => desc ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt)
            };

            var total = await query.LongCountAsync();
            var items = await query.Skip((q.Page - 1) * q.PageSize)
                                   .Take(q.PageSize)
                                   .Select(x => new PricingRuleListItemDto
                                   {
                                       PricingRuleId = x.PricingRuleId,
                                       ChargerType = x.ChargerType,
                                       PowerKw = x.PowerKw,
                                       TimeRange = x.TimeRange,
                                       PricePerKwh = x.PricePerKwh,
                                       IdleFeePerMin = x.IdleFeePerMin,
                                       Status = x.Status,
                                       CreatedAt = x.CreatedAt
                                   })
                                   .ToListAsync();

            return new PagedResult<PricingRuleListItemDto>
            {
                Page = q.Page,
                PageSize = q.PageSize,
                TotalItems = total,
                TotalPages = (int)Math.Ceiling(total / (double)q.PageSize),
                Items = items
            };
        }

        // 🟩 Lấy chi tiết
        public async Task<PricingRuleDetailDto?> GetByIdAsync(int id)
        {
            var rule = await _repo.GetByIdAsync(id);
            if (rule == null) return null;

            return new PricingRuleDetailDto
            {
                PricingRuleId = rule.PricingRuleId,
                ChargerType = rule.ChargerType,
                PowerKw = rule.PowerKw,
                TimeRange = rule.TimeRange,
                PricePerKwh = rule.PricePerKwh,
                IdleFeePerMin = rule.IdleFeePerMin,
                Status = rule.Status,
                CreatedAt = rule.CreatedAt,
                UpdatedAt = rule.UpdatedAt
            };
        }

        // 🟩 Tạo mới
        public async Task<string> CreateAsync(PricingRuleCreateDto dto)
        {
            var exists = await _repo.GetAll()
                .AnyAsync(x => x.ChargerType == dto.ChargerType &&
                               x.PowerKw == dto.PowerKw &&
                               x.TimeRange == dto.TimeRange &&
                               x.Status == "Active");

            if (exists)
                return "Đã tồn tại quy tắc giá đang hoạt động cho loại trụ và công suất này.";

            var entity = new PricingRule
            {
                ChargerType = dto.ChargerType,
                PowerKw = dto.PowerKw,
                TimeRange = dto.TimeRange,
                PricePerKwh = dto.PricePerKwh,
                IdleFeePerMin = dto.IdleFeePerMin,
                Status = dto.Status ?? "Active",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            await _repo.AddAsync(entity);
            await _repo.SaveAsync();

            return "Tạo quy tắc giá thành công!";
        }

        // 🟩 Cập nhật
        public async Task<string> UpdateAsync(int id, PricingRuleUpdateDto dto)
        {
            var rule = await _repo.GetByIdAsync(id);
            if (rule == null)
                return "Không tìm thấy quy tắc giá.";

            rule.ChargerType = dto.ChargerType;
            rule.PowerKw = dto.PowerKw;
            rule.TimeRange = dto.TimeRange;
            rule.PricePerKwh = dto.PricePerKwh;
            rule.IdleFeePerMin = dto.IdleFeePerMin;
            rule.Status = dto.Status;
            rule.UpdatedAt = DateTime.Now;

            await _repo.UpdateAsync(rule);
            await _repo.SaveAsync();

            return "Cập nhật quy tắc giá thành công!";
        }

        // 🟩 Đổi trạng thái
        public async Task<string> ChangeStatusAsync(int id, string newStatus)
        {
            var rule = await _repo.GetByIdAsync(id);
            if (rule == null)
                return "Không tìm thấy quy tắc giá.";

            rule.Status = newStatus;
            rule.UpdatedAt = DateTime.Now;

            await _repo.UpdateAsync(rule);
            await _repo.SaveAsync();

            return $"Đã đổi trạng thái quy tắc #{id} thành '{newStatus}'.";
        }

        // 🟩 Xoá
        public async Task<string> DeleteAsync(int id)
        {
            var rule = await _repo.GetByIdAsync(id);
            if (rule == null)
                return "Không tìm thấy quy tắc giá.";

            await _repo.DeleteAsync(rule);
            await _repo.SaveAsync();

            return "Xoá quy tắc giá thành công!";
        }
    }
}
