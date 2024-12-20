using Domain_Layer.Models;
using Application_Layer.Interfaces;
using System;
using System.Threading.Tasks;
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
    }
} 