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

        /// <summary>
        /// Hämtar bokningar som överlappar intervallet. Operativa queries får bara
        /// aktiva bokningar; sätt <paramref name="includeCancelled"/> = true för
        /// rapportering, där även avbokade (soft-deletade) rader ska med.
        /// </summary>
        Task<List<BookingModel>> GetByDateRangeAsync(DateTime start, DateTime end, bool includeCancelled = false);

        /// <summary>
        /// Aktiva bokningar som ska påminnas: start i (windowStart, windowEnd]
        /// och ingen påminnelse skickad ännu. Service är inkluderad.
        /// </summary>
        Task<List<BookingModel>> GetDueForReminderAsync(DateTime windowStart, DateTime windowEnd);
        Task<List<BookingModel>> GetByEmployeeAndRangeAsync(string employeeId, DateTime from, DateTime to);
        Task AddAsync(BookingModel booking);
        Task<bool> TryAddIfNoConflictAsync(BookingModel booking);
        Task<bool> HasConflictAsync(Guid excludeBookingId, string employeeId, DateTime start, DateTime end);
        Task UpdateAsync(BookingModel booking);
        Task<List<BookingModel>> GetAllAsync();
    }
}
