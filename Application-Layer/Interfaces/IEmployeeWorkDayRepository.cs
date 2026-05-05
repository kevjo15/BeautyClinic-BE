using Domain_Layer.Models;

namespace Application_Layer.Interfaces;

public interface IEmployeeWorkDayRepository
{
    Task<List<EmployeeWorkDayModel>> GetByEmployeeAndRangeAsync(string employeeId, DateOnly from, DateOnly to);
    Task SetWorkDaysForRangeAsync(string employeeId, DateOnly from, DateOnly to, List<EmployeeWorkDayModel> workDays);
}
