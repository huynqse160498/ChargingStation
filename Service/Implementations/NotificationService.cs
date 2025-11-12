using Repositories.DTOs.Notifications;
using Repositories.Interfaces;
using Repositories.Models;
using Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Services.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _repo;
        private readonly ICustomerRepository _customerRepo;
        private readonly ICompanyRepository _companyRepo;

        public NotificationService(
            INotificationRepository repo,
            ICustomerRepository customerRepo,
            ICompanyRepository companyRepo)
        {
            _repo = repo;
            _customerRepo = customerRepo;
            _companyRepo = companyRepo;
        }

        public async Task<IEnumerable<NotificationReadDto>> GetAllAsync()
            => (await _repo.GetAllAsync()).Select(MapToRead);

        public async Task<IEnumerable<NotificationReadDto>> GetByCustomerAsync(int customerId, bool includeArchived = false)
        {
            var list = await _repo.GetByCustomerAsync(customerId);
            if (!includeArchived) list = list.Where(x => !x.IsArchived).ToList();
            return list.Select(MapToRead);
        }

        public async Task<IEnumerable<NotificationReadDto>> GetByCompanyAsync(int companyId, bool includeArchived = false)
        {
            var list = await _repo.GetByCompanyAsync(companyId);
            if (!includeArchived) list = list.Where(x => !x.IsArchived).ToList();
            return list.Select(MapToRead);
        }

        public async Task<NotificationReadDto> GetByIdAsync(int id)
        {
            var noti = await _repo.GetByIdAsync(id) ?? throw new KeyNotFoundException("Không tìm thấy thông báo.");
            return MapToRead(noti);
        }

        public async Task<NotificationReadDto> CreateAsync(NotificationCreateDto dto)
        {
            var entity = BuildEntityFromDto(dto);
            var saved = await _repo.AddAsync(entity);
            return MapToRead(saved);
        }

        public async Task MarkAsReadAsync(int id)
        {
            var noti = await _repo.GetByIdAsync(id) ?? throw new KeyNotFoundException("Không tìm thấy thông báo.");
            noti.IsRead = true;
            await _repo.UpdateAsync(noti);
        }

        public async Task ArchiveAsync(int id)
        {
            var noti = await _repo.GetByIdAsync(id) ?? throw new KeyNotFoundException("Không tìm thấy thông báo.");
            noti.IsArchived = true;
            await _repo.UpdateAsync(noti);
        }

        public async Task DeleteAsync(int id)
        {
            var noti = await _repo.GetByIdAsync(id) ?? throw new KeyNotFoundException("Không tìm thấy thông báo.");
            await _repo.DeleteAsync(noti);
        }

        // ===================== Admin-special =====================

        public async Task<NotificationReadDto> AdminSendToCustomerAsync(AdminSendToCustomerDto dto)
        {
            // validate
            var customer = await _customerRepo.GetByIdAsync(dto.CustomerId)
                           ?? throw new KeyNotFoundException("Không tìm thấy khách hàng.");

            var create = new NotificationCreateDto
            {
                CustomerId = dto.CustomerId,
                Title = dto.Title,
                Message = dto.Message,
                Type = dto.Type,
                Priority = dto.Priority,
                SenderAdminId = dto.SenderAdminId,
                BookingId = dto.BookingId,
                InvoiceId = dto.InvoiceId,
                SubscriptionId = dto.SubscriptionId
            };

            var entity = BuildEntityFromDto(create);
            var saved = await _repo.AddAsync(entity);
            return MapToRead(saved);
        }

        public async Task<NotificationReadDto> AdminSendToCompanyAsync(AdminSendToCompanyDto dto)
        {
            var company = await _companyRepo.GetByIdAsync(dto.CompanyId)
                          ?? throw new KeyNotFoundException("Không tìm thấy công ty.");

            var create = new NotificationCreateDto
            {
                CompanyId = dto.CompanyId,
                Title = dto.Title,
                Message = dto.Message,
                Type = dto.Type,
                Priority = dto.Priority,
                SenderAdminId = dto.SenderAdminId,
                BookingId = dto.BookingId,
                InvoiceId = dto.InvoiceId,
                SubscriptionId = dto.SubscriptionId
            };

            var entity = BuildEntityFromDto(create);
            var saved = await _repo.AddAsync(entity);
            return MapToRead(saved);
        }

        public async Task<int> AdminBroadcastAsync(AdminBroadcastDto dto)
        {
            // Audience: All | Customers | Companies
            var toCustomers = dto.Audience is "All" or "Customers";
            var toCompanies = dto.Audience is "All" or "Companies";

            var notifications = new List<Notification>();

            if (toCustomers)
            {
                var customers = await _customerRepo.GetAllAsync();
                notifications.AddRange(customers.Select(c => new Notification
                {
                    CustomerId = c.CustomerId,
                    Title = dto.Title,
                    Message = dto.Message,
                    Type = dto.Type,
                    Priority = dto.Priority,
                    SenderAdminId = dto.SenderAdminId,
                    BookingId = dto.BookingId,
                    InvoiceId = dto.InvoiceId,
                    SubscriptionId = dto.SubscriptionId,
                    ActionUrl = BuildActionUrl(dto.BookingId, dto.InvoiceId, dto.SubscriptionId)
                }));
            }

            if (toCompanies)
            {
                var companies = await _companyRepo.GetAllAsync();
                notifications.AddRange(companies.Select(c => new Notification
                {
                    CompanyId = c.CompanyId,
                    Title = dto.Title,
                    Message = dto.Message,
                    Type = dto.Type,
                    Priority = dto.Priority,
                    SenderAdminId = dto.SenderAdminId,
                    BookingId = dto.BookingId,
                    InvoiceId = dto.InvoiceId,
                    SubscriptionId = dto.SubscriptionId,
                    ActionUrl = BuildActionUrl(dto.BookingId, dto.InvoiceId, dto.SubscriptionId)
                }));
            }

            int count = 0;
            foreach (var n in notifications)
            {
                await _repo.AddAsync(n);
                count++;
            }
            return count;
        }

        // ======================= helpers =======================

        private static Notification BuildEntityFromDto(NotificationCreateDto dto)
        {
            return new Notification
            {
                CustomerId = dto.CustomerId,
                CompanyId = dto.CompanyId,
                BookingId = dto.BookingId,
                InvoiceId = dto.InvoiceId,
                SubscriptionId = dto.SubscriptionId,
                Title = dto.Title,
                Message = dto.Message,
                Type = dto.Type,
                Priority = dto.Priority,
                SenderAdminId = dto.SenderAdminId,
                ActionUrl = BuildActionUrl(dto.BookingId, dto.InvoiceId, dto.SubscriptionId)
            };
        }

        private static string? BuildActionUrl(int? bookingId, int? invoiceId, int? subscriptionId)
        {
            if (invoiceId.HasValue) return $"/invoices/{invoiceId}";
            if (bookingId.HasValue) return $"/user/history";
            if (subscriptionId.HasValue) return $"/subscriptions/{subscriptionId}";
            return null;
        }

        private static NotificationReadDto MapToRead(Notification n) => new()
        {
            NotificationId = n.NotificationId,
            Title = n.Title,
            Message = n.Message,
            Type = n.Type,
            Priority = n.Priority,
            ActionUrl = n.ActionUrl,
            IsRead = n.IsRead,
            IsArchived = n.IsArchived,
            CreatedAt = n.CreatedAt
        };
    }
}
