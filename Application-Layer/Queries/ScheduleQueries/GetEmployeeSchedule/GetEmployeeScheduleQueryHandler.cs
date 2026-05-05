using Application_Layer.DTOs;
using Application_Layer.Interfaces;
using MediatR;

namespace Application_Layer.Queries.ScheduleQueries.GetEmployeeSchedule
{
    public class GetEmployeeScheduleQueryHandler : IRequestHandler<GetEmployeeScheduleQuery, List<EmployeeScheduleDTO>>
    {
        private readonly IEmployeeScheduleRepository _scheduleRepository;

        public GetEmployeeScheduleQueryHandler(IEmployeeScheduleRepository scheduleRepository)
        {
            _scheduleRepository = scheduleRepository;
        }

        public async Task<List<EmployeeScheduleDTO>> Handle(GetEmployeeScheduleQuery request, CancellationToken cancellationToken)
        {
            var schedule = await _scheduleRepository.GetByEmployeeIdAsync(request.EmployeeId);

            return schedule
                .OrderBy(s => s.DayOfWeek)
                .Select(s => new EmployeeScheduleDTO
                {
                    DayOfWeek = s.DayOfWeek,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime
                })
                .ToList();
        }
    }
}
