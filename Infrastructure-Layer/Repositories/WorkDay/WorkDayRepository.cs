using Application_Layer.Interfaces;
using Domain_Layer.Models;
using Infrastructure_Layer.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure_Layer.Repositories.WorkDay;

public class WorkDayRepository : IEmployeeWorkDayRepository
{
    private readonly ElsaBeautyDbContext _context;

    public WorkDayRepository(ElsaBeautyDbContext context)
    {
        _context = context;
    }

    public async Task<List<EmployeeWorkDayModel>> GetByEmployeeAndRangeAsync(string employeeId, DateOnly from, DateOnly to)
    {
        return await _context.EmployeeWorkDays
            .Where(w => w.EmployeeId == employeeId && w.Date >= from && w.Date <= to)
            .ToListAsync();
    }

    public async Task SetWorkDaysForRangeAsync(string employeeId, DateOnly from, DateOnly to, List<EmployeeWorkDayModel> workDays)
    {
        var existing = await _context.EmployeeWorkDays
            .Where(w => w.EmployeeId == employeeId && w.Date >= from && w.Date <= to)
            .ToListAsync();

        _context.EmployeeWorkDays.RemoveRange(existing);
        await _context.EmployeeWorkDays.AddRangeAsync(workDays);
        await _context.SaveChangesAsync();
    }
}
