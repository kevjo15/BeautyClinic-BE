using Domain_Layer.Models;

namespace Application_Layer.Interfaces
{
    public interface IEmployeeScheduleRepository
    {
        Task<List<EmployeeScheduleModel>> GetByEmployeeIdAsync(string employeeId);
        Task SetScheduleAsync(string employeeId, List<EmployeeScheduleModel> schedule);
    }
}
