using Repositories.Models;

namespace Repositories.Interfaces
{
    public interface IChargingSessionRepository
    {
        Task<List<ChargingSession>> GetAllAsync();
        Task<ChargingSession?> GetByIdAsync(int id);
        Task AddAsync(ChargingSession session);
        Task UpdateAsync(ChargingSession session);
        Task DeleteAsync(ChargingSession session);
    }
}
