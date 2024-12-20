using Domain_Layer.Models;
using System;
using System.Threading.Tasks;

namespace Application_Layer.Interfaces
{
    public interface IBookingRepository
    {
    Task<BookingModel> GetByIdAsync(Guid id);
    Task<List<BookingModel>> GetByUserIdAsync(string userId);
    Task AddAsync(BookingModel booking);
    Task UpdateAsync(BookingModel booking);
    Task DeleteAsync(Guid id);
    }
} 