using Application_Layer.Interfaces;
using Domain_Layer.Models;
using Infrastructure_Layer.Database;
using Infrastructure_Layer.Identity;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Infrastructure_Layer.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly ElsaBeautyDbContext _context;

        public BookingRepository(ElsaBeautyDbContext context)
        {
            _context = context;
        }

        public async Task<BookingModel?> GetByIdAsync(Guid id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Service)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking != null)
            {
                await PopulateUsersAsync(new[] { booking });
            }

            return booking;
        }

        public async Task AddAsync(BookingModel booking)
        {
            await _context.Bookings.AddAsync(booking);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> TryAddIfNoConflictAsync(BookingModel booking)
        {
            try
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

                var conflict = await _context.Bookings.AnyAsync(b =>
                    b.StartTime < booking.EndTime && b.EndTime > booking.StartTime);

                if (conflict)
                {
                    await transaction.RollbackAsync();
                    return false;
                }

                await _context.Bookings.AddAsync(booking);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 1205)
            {
                // Deadlock - another transaction won, treat as conflict
                return false;
            }
        }

        public async Task UpdateAsync(BookingModel booking)
        {
            _context.Entry(booking).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return;
            }

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();
        }

        public async Task<List<BookingModel>> GetByUserIdAsync(string userId)
        {
            var bookings = await _context.Bookings
                .Include(b => b.Service)
                .Where(b => b.UserId == userId)
                .ToListAsync();

            await PopulateUsersAsync(bookings);
            return bookings;
        }

        public async Task<List<BookingModel>> GetByDateRangeAsync(DateTime start, DateTime end)
        {
            var bookings = await _context.Bookings
                .Include(b => b.Service)
                .Where(b => (b.StartTime >= start && b.StartTime < end) ||
                           (b.EndTime > start && b.EndTime <= end) ||
                           (b.StartTime <= start && b.EndTime >= end))
                .ToListAsync();

            await PopulateUsersAsync(bookings);
            return bookings;
        }

        public async Task<List<BookingModel>> GetByEmployeeAndRangeAsync(string employeeId, DateTime from, DateTime to)
        {
            var bookings = await _context.Bookings
                .Include(b => b.Service)
                .Where(b => b.EmployeeId == employeeId &&
                    ((b.StartTime >= from && b.StartTime < to) ||
                     (b.EndTime > from && b.EndTime <= to) ||
                     (b.StartTime <= from && b.EndTime >= to)))
                .ToListAsync();

            await PopulateUsersAsync(bookings);
            return bookings;
        }

        public async Task<List<BookingModel>> GetAllAsync()
        {
            var bookings = await _context.Bookings
                .Include(b => b.Service)
                .ToListAsync();

            await PopulateUsersAsync(bookings);
            return bookings;
        }

        private async Task PopulateUsersAsync(IEnumerable<BookingModel> bookings)
        {
            var ids = bookings
                .SelectMany(booking => new[] { booking.UserId, booking.EmployeeId })
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();

            if (ids.Count == 0)
            {
                return;
            }

            var users = await _context.Users
                .Where(user => ids.Contains(user.Id))
                .ToDictionaryAsync(user => user.Id, MapToDomainUser);

            foreach (var booking in bookings)
            {
                if (users.TryGetValue(booking.UserId, out var user))
                {
                    booking.User = user;
                }

                if (!string.IsNullOrWhiteSpace(booking.EmployeeId) &&
                    users.TryGetValue(booking.EmployeeId, out var employee))
                {
                    booking.Employee = employee;
                }
            }
        }

        private static UserModel MapToDomainUser(ApplicationUser user)
        {
            return new UserModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsDeleted = user.IsDeleted
            };
        }
    }
}
