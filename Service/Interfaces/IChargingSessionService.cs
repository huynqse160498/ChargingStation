using Repositories.DTOs;
using Repositories.Models;

namespace Services.Interfaces
{
    public interface IChargingSessionService
    {
        // 🔹 Bắt đầu sạc (có thể có hoặc không Booking)
        Task<ChargingSession> StartSessionAsync(ChargingSessionCreateDto dto);

        // 🔹 Kết thúc phiên sạc
        Task<ChargingSession> EndSessionAsync(ChargingSessionEndDto dto);
        Task<ChargingSession> StartGuestSessionAsync(GuestChargingStartDto dto);
        Task<ChargingSession> EndGuestSessionAsync(GuestChargingEndDto dto);

        // 🔹 CRUD cơ bản
        Task<List<ChargingSession>> GetAllAsync();
        Task<ChargingSession?> GetByIdAsync(int id);
        Task DeleteAsync(int id);
    }
}
