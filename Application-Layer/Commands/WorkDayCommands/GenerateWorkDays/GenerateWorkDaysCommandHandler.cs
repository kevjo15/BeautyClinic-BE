using Application_Layer.Interfaces;
using Domain_Layer.Common;
using Domain_Layer.Models;
using MediatR;

namespace Application_Layer.Commands.WorkDayCommands.GenerateWorkDays;

public class GenerateWorkDaysCommandHandler : IRequestHandler<GenerateWorkDaysCommand, OperationResult>
{
    private readonly IEmployeeWorkDayRepository _workDayRepository;
    private readonly IEmployeeScheduleRepository _scheduleRepository;

    public GenerateWorkDaysCommandHandler(
        IEmployeeWorkDayRepository workDayRepository,
        IEmployeeScheduleRepository scheduleRepository)
    {
        _workDayRepository = workDayRepository;
        _scheduleRepository = scheduleRepository;
    }

    public async Task<OperationResult> Handle(GenerateWorkDaysCommand request, CancellationToken cancellationToken)
    {
        if (!DateOnly.TryParse(request.Dto.From, out var from) ||
            !DateOnly.TryParse(request.Dto.To, out var to))
            return OperationResult.Failure("Ogiltigt datumformat. Använd yyyy-MM-dd.");

        if (to < from)
            return OperationResult.Failure("Slutdatum måste vara efter startdatum.");

        if ((to.DayNumber - from.DayNumber) > 90)
            return OperationResult.Failure("Max 90 dagar per generering.");

        var schedules = await _scheduleRepository.GetByEmployeeIdAsync(request.EmployeeId);
        if (schedules.Count == 0)
            return OperationResult.Failure("Inget basschema finns. Skapa ett basschema först.");

        var scheduleByDay = schedules.ToDictionary(s => s.DayOfWeek);
        var workDays = new List<EmployeeWorkDayModel>();

        for (var date = from; date <= to; date = date.AddDays(1))
        {
            if (!scheduleByDay.TryGetValue(date.DayOfWeek, out var daySchedule))
                continue;

            workDays.Add(new EmployeeWorkDayModel
            {
                Id = Guid.NewGuid(),
                EmployeeId = request.EmployeeId,
                Date = date,
                StartTime = daySchedule.StartTime,
                EndTime = daySchedule.EndTime,
            });
        }

        await _workDayRepository.SetWorkDaysForRangeAsync(request.EmployeeId, from, to, workDays);
        return OperationResult.Success();
    }
}
