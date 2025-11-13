using Repositories.DTOs.Subscriptions;
using Repositories.Interfaces;
using Repositories.Models;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services.Implementations
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ISubscriptionRepository _repo;
        private readonly ISubscriptionPlanRepository _planRepo;
        private readonly INotificationRepository _notiRepo;

        public SubscriptionService(
            ISubscriptionRepository repo,
            ISubscriptionPlanRepository planRepo,
            INotificationRepository notiRepo)
        {
            _repo = repo;
            _planRepo = planRepo;
            _notiRepo = notiRepo;
        }

        // ======================= [ VALIDATION HELPERS ] =======================
        private static void EnsureExactlyOneOwner(int? customerId, int? companyId)
        {
            var count = (customerId.HasValue ? 1 : 0) + (companyId.HasValue ? 1 : 0);
            if (count != 1)
                throw new InvalidOperationException("Phải chọn đúng 1: Customer hoặc Company.");
        }

        private static void EnsureOwnerMatchesPlan(SubscriptionPlan plan, int? customerId, int? companyId)
        {
            if (plan.IsForCompany)
            {
                if (!companyId.HasValue)
                    throw new InvalidOperationException("Gói này chỉ dành cho doanh nghiệp.");
            }
            else
            {
                if (!customerId.HasValue)
                    throw new InvalidOperationException("Gói này chỉ dành cho cá nhân.");
            }
        }

        // ======================= [ GET - ALL ] =======================
        public async Task<IEnumerable<SubscriptionReadDto>> GetAllAsync()
            => (await _repo.GetAllAsync()).Select(MapToRead);

        public async Task<SubscriptionReadDto> GetByIdAsync(int id)
        {
            var s = await _repo.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Subscription không tồn tại.");
            return MapToRead(s);
        }

        // ======================= [ CREATE ] =======================
        public async Task<SubscriptionReadDto> CreateAsync(SubscriptionCreateDto dto)
        {
            EnsureExactlyOneOwner(dto.CustomerId, dto.CompanyId);

            var plan = await _planRepo.GetByIdAsync(dto.SubscriptionPlanId)
                ?? throw new KeyNotFoundException("Không tìm thấy gói dịch vụ.");

            EnsureOwnerMatchesPlan(plan, dto.CustomerId, dto.CompanyId);

            if (dto.CustomerId.HasValue)
            {
                if (await _repo.HasCurrentByCustomerAsync(dto.CustomerId.Value))
                    throw new InvalidOperationException("Khách hàng đã có gói đang hoạt động.");
            }
            else if (dto.CompanyId.HasValue)
            {
                if (await _repo.HasCurrentByCompanyAsync(dto.CompanyId.Value))
                    throw new InvalidOperationException("Công ty đã có gói đang hoạt động.");
            }

            var sub = new Subscription
            {
                SubscriptionPlanId = dto.SubscriptionPlanId,
                CustomerId = dto.CustomerId,
                CompanyId = dto.CompanyId,
                BillingCycle = dto.BillingCycle ?? "Monthly",
                AutoRenew = false,
                Status = "Pending",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            var saved = await _repo.AddAsync(sub);

            // 🔹 Gửi thông báo tự động
            await _notiRepo.AddAsync(new Notification
            {
                CustomerId = sub.CustomerId,
                CompanyId = sub.CompanyId,
                SubscriptionId = sub.SubscriptionId,
                Title = "Đăng ký gói dịch vụ thành công",
                Message = $"Bạn đã đăng ký gói '{plan.PlanName}' thành công. Vui lòng thanh toán để kích hoạt.",
                Type = "Subscription",
                Priority = "Normal",
                ActionUrl = $"/invoiceSummary"
            });

            saved.SubscriptionPlan = plan;
            return MapToRead(saved);
        }

        // ======================= [ UPDATE ] =======================
        public async Task<SubscriptionReadDto> UpdateAsync(int id, SubscriptionUpdateDto dto)
        {
            var e = await _repo.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Subscription không tồn tại.");

            if (dto.SubscriptionPlanId is int planId)
            {
                var plan = await _planRepo.GetByIdAsync(planId)
                    ?? throw new KeyNotFoundException("Subscription plan không tồn tại.");

                EnsureOwnerMatchesPlan(plan, e.CustomerId, e.CompanyId);
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

        // ======================= [ DELETE ] =======================
        public async Task DeleteAsync(int id)
        {
            var e = await _repo.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Subscription không tồn tại.");
            await _repo.DeleteAsync(e);
        }

        // ======================= [ MAPPING ] =======================
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

        public async Task<SubscriptionReadDto> UpdateStatusAsync(int id, string status)
        {
            var sub = await _repo.GetByIdAsync(id)
                      ?? throw new KeyNotFoundException("Không tìm thấy gói dịch vụ.");

            // TODO: validate transition nếu cần (ví dụ không cho quay lại từ CANCELED)
            sub.Status = status;
            sub.UpdatedAt = DateTime.Now;

            await _repo.UpdateAsync(sub);
            return MapToRead(sub);
        }
    }
}
