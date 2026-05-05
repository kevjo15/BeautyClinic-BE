using Application_Layer.DTOs;
using MediatR;

namespace Application_Layer.Queries.WorkDayQueries.GetWorkDays;

public class GetWorkDaysQuery : IRequest<List<EmployeeWorkDayDTO>>
{
    public string EmployeeId { get; }
    public DateOnly From { get; }
    public DateOnly To { get; }

    public GetWorkDaysQuery(string employeeId, DateOnly from, DateOnly to)
    {
        EmployeeId = employeeId;
        From = from;
        To = to;
    }
}
