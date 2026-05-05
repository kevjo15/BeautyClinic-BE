using Application_Layer.DTOs;
using MediatR;

namespace Application_Layer.Queries.ScheduleQueries.GetEmployeeSchedule
{
    public class GetEmployeeScheduleQuery : IRequest<List<EmployeeScheduleDTO>>
    {
        public string EmployeeId { get; }

        public GetEmployeeScheduleQuery(string employeeId)
        {
            EmployeeId = employeeId;
        }
    }
}
