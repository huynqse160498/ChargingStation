using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;
using Repositories.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories.Implementations
{
    public class BookingRepository : IBookingRepository
    {
        private readonly ChargeStationContext _context;

        public BookingRepository(ChargeStationContext context)
        {
            _context = context;
        }

        public IQueryable<Booking> GetAll()
        {
            return _context.Bookings.AsNoTracking();
        }

        public async Task<Booking?> GetByIdAsync(int id)
        {
            return await _context.Bookings.FindAsync(id);
        }

        public async Task AddAsync(Booking booking)
        {
            await _context.Bookings.AddAsync(booking);
        }

        public Task UpdateAsync(Booking booking)
        {
            _context.Bookings.Update(booking);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Booking booking)
        {
            _context.Bookings.Remove(booking);
            return Task.CompletedTask;
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
