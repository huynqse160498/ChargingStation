using Repositories.DTOs.SubscriptionPlans;
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
    public class SubscriptionPlanService : ISubscriptionPlanService
    {
        private readonly ISubscriptionPlanRepository _repo;
        public SubscriptionPlanService(ISubscriptionPlanRepository repo) { _repo = repo; } // NEW

        public async Task<IEnumerable<SubscriptionPlanReadDto>> GetAllAsync()
            => (await _repo.GetAllAsync()).Select(MapToRead);                              // NEW

        public async Task<SubscriptionPlanReadDto> GetByIdAsync(int id)
        {
            var e = await _repo.GetByIdAsync(id) ?? throw new KeyNotFoundException("Plan không tồn tại.");
            return MapToRead(e);                                                           // NEW
        }

        public async Task<SubscriptionPlanReadDto> CreateAsync(SubscriptionPlanCreateDto dto)
        {
            if (await _repo.ExistsNameAsync(dto.PlanName))
                throw new InvalidOperationException("Tên gói đã tồn tại.");               // NEW

            var now = DateTime.Now;                                                       // NEW

            var e = new SubscriptionPlan
            {
                PlanName = dto.PlanName.Trim(),
                Description = dto.Description ?? string.Empty,
                Category = (dto.Category ?? "Individual").Trim(),
                PriceMonthly = dto.PriceMonthly,
                PriceYearly = dto.PriceYearly,
                DiscountPercent = dto.DiscountPercent,
                FreeIdleMinutes = dto.FreeIdleMinutes,
                Benefits = dto.Benefits ?? string.Empty,
                IsForCompany = dto.IsForCompany,
                Status = (dto.Status == "Inactive") ? "Inactive" : "Active",
                CreatedAt = now,
                UpdatedAt = now
            };

            var saved = await _repo.AddAsync(e);                                          // NEW
            return MapToRead(saved);                                                      // NEW
        }

        public async Task<SubscriptionPlanReadDto> UpdateAsync(int id, SubscriptionPlanUpdateDto dto)
        {
            var e = await _repo.GetByIdAsync(id) ?? throw new KeyNotFoundException("Plan không tồn tại."); // NEW

            if (!string.IsNullOrWhiteSpace(dto.PlanName) && dto.PlanName != e.PlanName)
            {
                if (await _repo.ExistsNameAsync(dto.PlanName, id))
                    throw new InvalidOperationException("Tên gói đã tồn tại.");           // NEW
                e.PlanName = dto.PlanName.Trim();
            }
            if (dto.Description != null) e.Description = dto.Description;
            if (!string.IsNullOrWhiteSpace(dto.Category)) e.Category = dto.Category!;   
            if (dto.PriceMonthly is decimal pm) e.PriceMonthly = pm; //fix
            if (dto.PriceYearly.HasValue) e.PriceYearly = dto.PriceYearly.Value;
            if (dto.DiscountPercent.HasValue) e.DiscountPercent = dto.DiscountPercent.Value;
            if (dto.FreeIdleMinutes.HasValue) e.FreeIdleMinutes = dto.FreeIdleMinutes.Value;
            if (dto.Benefits != null) e.Benefits = dto.Benefits;
            if (dto.IsForCompany is bool b) e.IsForCompany = b; //fix
            if (!string.IsNullOrWhiteSpace(dto.Status)) e.Status = dto.Status!;
            e.UpdatedAt = DateTime.Now;                                                   // NEW

            var saved = await _repo.UpdateAsync(e);                                       // NEW
            return MapToRead(saved);                                                      // NEW
        }

        public async Task DeleteAsync(int id)
        {
            var e = await _repo.GetByIdAsync(id) ?? throw new KeyNotFoundException("Plan không tồn tại.");
            await _repo.DeleteAsync(e);                                                   // NEW
        }

        // NEW
        private static SubscriptionPlanReadDto MapToRead(SubscriptionPlan x) => new SubscriptionPlanReadDto
        {
            SubscriptionPlanId = x.SubscriptionPlanId,
            PlanName = x.PlanName,
            Description = x.Description,
            Category = x.Category,
            PriceMonthly = x.PriceMonthly,
            PriceYearly = x.PriceYearly,
            DiscountPercent = x.DiscountPercent,
            FreeIdleMinutes = x.FreeIdleMinutes,
            Benefits = x.Benefits,
            IsForCompany = x.IsForCompany,
            Status = x.Status,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt
            };
    }
}
