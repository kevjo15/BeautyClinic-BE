using Application_Layer.Interfaces;
using Domain_Layer.Models;
using Infrastructure_Layer.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure_Layer.Repositories.Schedule
{
    public class ScheduleRepository : IEmployeeScheduleRepository
    {
        private readonly ElsaBeautyDbContext _context;

        public ScheduleRepository(ElsaBeautyDbContext context)
        {
            _context = context;
        }

        public async Task<List<EmployeeScheduleModel>> GetByEmployeeIdAsync(string employeeId)
        {
            return await _context.EmployeeSchedules
                .Where(s => s.EmployeeId == employeeId)
                .ToListAsync();
        }

        public async Task SetScheduleAsync(string employeeId, List<EmployeeScheduleModel> schedule)
        {
            var existing = await _context.EmployeeSchedules
                .Where(s => s.EmployeeId == employeeId)
                .ToListAsync();

            _context.EmployeeSchedules.RemoveRange(existing);
            await _context.EmployeeSchedules.AddRangeAsync(schedule);
            await _context.SaveChangesAsync();
        }
    }
}
