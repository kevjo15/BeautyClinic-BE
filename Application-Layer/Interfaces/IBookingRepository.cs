using Domain_Layer.Common;
using Domain_Layer.Models;
using System;
using System.Threading.Tasks;

namespace Application_Layer.Interfaces
{
    public interface IBookingRepository
    {
        Task<BookingModel?> GetByIdAsync(Guid id);
        Task<List<BookingModel>> GetByUserIdAsync(string userId);
        Task<List<BookingModel>> GetByDateRangeAsync(DateTime start, DateTime end);
        Task<List<BookingModel>> GetByEmployeeAndRangeAsync(string employeeId, DateTime from, DateTime to);
        Task AddAsync(BookingModel booking);
        Task<bool> TryAddIfNoConflictAsync(BookingModel booking);
        Task<bool> HasConflictAsync(Guid excludeBookingId, string employeeId, DateTime start, DateTime end);
        Task UpdateAsync(BookingModel booking);
        Task DeleteAsync(Guid id);
        Task<List<BookingModel>> GetAllAsync();
    }
}
