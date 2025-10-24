using Repositories.DTOs.Subscriptions;
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
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ISubscriptionRepository _repo;
        private readonly ISubscriptionPlanRepository _planRepo;
        private readonly INotificationRepository _notiRepo;
        public SubscriptionService(ISubscriptionRepository repo, ISubscriptionPlanRepository planRepo, INotificationRepository notiRepo)
        {
            _repo = repo;
            _planRepo = planRepo;
            _notiRepo = notiRepo;
        }

        // ======================= [ GET - ALL ] =======================
        public async Task<IEnumerable<SubscriptionReadDto>> GetAllAsync()
            => (await _repo.GetAllAsync()).Select(MapToRead);

        // ======================= [ GET - BY ID ] =====================
        public async Task<SubscriptionReadDto> GetByIdAsync(int id)
        {
            var s = await _repo.GetByIdAsync(id) ?? throw new KeyNotFoundException("Subscription không tồn tại.");
            return MapToRead(s);
        }

        // ======================= [ CREATE ] ==========================
        public async Task<SubscriptionReadDto> CreateAsync(SubscriptionCreateDto dto)
        {
            var plan = await _planRepo.GetByIdAsync(dto.SubscriptionPlanId)
                       ?? throw new KeyNotFoundException("Subscription plan không tồn tại.");

            // ✅ Gói mới tạo -> trạng thái Pending, chưa kích hoạt
            var sub = new Subscription
            {
                SubscriptionPlanId = dto.SubscriptionPlanId,
                CustomerId = dto.CustomerId ?? 0,
                CompanyId = dto.CompanyId,
                BillingCycle = dto.BillingCycle ?? "Monthly",
                AutoRenew = false, // ❗ manual renew
                Status = "Pending", // ❗ Chưa kích hoạt
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            var saved = await _repo.AddAsync(sub);
            await _notiRepo.AddAsync(new Notification
            {
                CustomerId = sub.CustomerId,
                CompanyId = sub.CompanyId,
                SubscriptionId = sub.SubscriptionId,
                Title = "Đăng ký gói dịch vụ thành công",
                Message = $"Bạn đã đăng ký gói '{plan.PlanName}' thành công. Thời hạn sử dụng đến {sub.EndDate:dd/MM/yyyy}.",
                Type = "Subscription",
                Priority = "Normal",
                ActionUrl = $"/subscriptions/{sub.SubscriptionId}"
            });

            saved.SubscriptionPlan = plan;
            return MapToRead(saved);
        }


        // ======================= [ UPDATE ] ==========================
        public async Task<SubscriptionReadDto> UpdateAsync(int id, SubscriptionUpdateDto dto)
        {
            var e = await _repo.GetByIdAsync(id) ?? throw new KeyNotFoundException("Subscription không tồn tại.");

            // planId
            if (dto.SubscriptionPlanId is int planId)                    // FIX
            {
                var plan = await _planRepo.GetByIdAsync(planId)
           ?? throw new KeyNotFoundException("Subscription plan không tồn tại.");
                e.SubscriptionPlanId = plan.SubscriptionPlanId;
            }

            if (!string.IsNullOrWhiteSpace(dto.BillingCycle))
                e.BillingCycle = dto.BillingCycle;

            if (dto.AutoRenew is bool b)
                e.AutoRenew = b;

            if (dto.StartDate is DateTime start)
                e.StartDate = start;

            if (!string.IsNullOrWhiteSpace(dto.Status))
                e.Status = dto.Status;

            e.UpdatedAt = DateTime.Now;
            var saved = await _repo.UpdateAsync(e);
            return MapToRead(saved);
        }

        // ======================= [ DELETE ] ==========================
        public async Task DeleteAsync(int id)
        {
            var e = await _repo.GetByIdAsync(id) ?? throw new KeyNotFoundException("Subscription không tồn tại.");
            await _repo.DeleteAsync(e);
        }

        // ======================= [ MAPPING ] =========================
        private static SubscriptionReadDto MapToRead(Subscription s) => new SubscriptionReadDto
        {
            SubscriptionId = s.SubscriptionId,
            SubscriptionPlanId = s.SubscriptionPlanId,
            PlanName = s.SubscriptionPlan?.PlanName ?? "",
            CustomerId = s.CustomerId,
            CompanyId = s.CompanyId,
            StartDate = s.StartDate,
            EndDate = s.EndDate,
            BillingCycle = s.BillingCycle,
            AutoRenew = s.AutoRenew,
            NextBillingDate = s.NextBillingDate,
            Status = s.Status
        };
    }
}

