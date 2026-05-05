using Application_Layer.DTOs;
using Application_Layer.Interfaces;
using MediatR;

namespace Application_Layer.Queries.WorkDayQueries.GetWorkDays;

public class GetWorkDaysQueryHandler : IRequestHandler<GetWorkDaysQuery, List<EmployeeWorkDayDTO>>
{
    private readonly IEmployeeWorkDayRepository _workDayRepository;

    public GetWorkDaysQueryHandler(IEmployeeWorkDayRepository workDayRepository)
    {
        _workDayRepository = workDayRepository;
    }

    public async Task<List<EmployeeWorkDayDTO>> Handle(GetWorkDaysQuery request, CancellationToken cancellationToken)
    {
        var workDays = await _workDayRepository.GetByEmployeeAndRangeAsync(request.EmployeeId, request.From, request.To);

        return workDays
            .OrderBy(w => w.Date)
            .Select(w => new EmployeeWorkDayDTO
            {
                Date = w.Date.ToString("yyyy-MM-dd"),
                StartTime = w.StartTime.ToString(@"hh\:mm\:ss"),
                EndTime = w.EndTime.ToString(@"hh\:mm\:ss"),
            })
            .ToList();
    }
}
