using Application_Layer.Interfaces;
using Microsoft.EntityFrameworkCore;
using Infrastructure_Layer.Database;

namespace Infrastructure_Layer.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly ElsaBeautyDbContext _context;

        public BookingRepository(ElsaBeautyDbContext context)
        {
            _context = context;
        }

        public async Task<BookingModel> GetByIdAsync(Guid id)
        {
            return await _context.Bookings.FindAsync(id);
        }

        public async Task AddAsync(BookingModel booking)
        {
            await _context.Bookings.AddAsync(booking);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(BookingModel booking)
        {
            _context.Entry(booking).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();
        }

        public async Task<List<BookingModel>> GetByUserIdAsync(string userId)
        {
            return await _context.Bookings
                .Where(b => b.UserId == userId)
                .ToListAsync();
        }

        public async Task<List<BookingModel>> GetByDateRangeAsync(DateTime start, DateTime end)
        {
            return await _context.Bookings
                .Where(b => (b.StartTime >= start && b.StartTime < end) ||
                           (b.EndTime > start && b.EndTime <= end) ||
                           (b.StartTime <= start && b.EndTime >= end))
                .ToListAsync();
        }

        public async Task<List<BookingModel>> GetAllAsync()
        {
            return await _context.Bookings.ToListAsync();
        }
    }
}
