using Application_Layer.Interfaces;
using Domain_Layer.Common;
using Domain_Layer.Models;
using MediatR;

namespace Application_Layer.Commands.ScheduleCommands.SetEmployeeSchedule
{
    public class SetEmployeeScheduleCommandHandler : IRequestHandler<SetEmployeeScheduleCommand, OperationResult>
    {
        private readonly IEmployeeScheduleRepository _scheduleRepository;

        public SetEmployeeScheduleCommandHandler(IEmployeeScheduleRepository scheduleRepository)
        {
            _scheduleRepository = scheduleRepository;
        }

        public async Task<OperationResult> Handle(SetEmployeeScheduleCommand request, CancellationToken cancellationToken)
        {
            var hasDuplicateDay = request.Schedule
                .GroupBy(s => s.DayOfWeek)
                .Any(g => g.Count() > 1);

            if (hasDuplicateDay)
                return OperationResult.Failure("Schedule cannot contain duplicate days.");

            var scheduleModels = request.Schedule.Select(s => new EmployeeScheduleModel
            {
                Id = Guid.NewGuid(),
                EmployeeId = request.EmployeeId,
                DayOfWeek = s.DayOfWeek,
                StartTime = s.StartTime,
                EndTime = s.EndTime
            }).ToList();

            await _scheduleRepository.SetScheduleAsync(request.EmployeeId, scheduleModels);
            return OperationResult.Success();
        }
    }
}
