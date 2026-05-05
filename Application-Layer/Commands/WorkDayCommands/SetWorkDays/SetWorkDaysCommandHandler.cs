using Application_Layer.Interfaces;
using Domain_Layer.Common;
using Domain_Layer.Models;
using MediatR;

namespace Application_Layer.Commands.WorkDayCommands.SetWorkDays;

public class SetWorkDaysCommandHandler : IRequestHandler<SetWorkDaysCommand, OperationResult>
{
    private readonly IEmployeeWorkDayRepository _workDayRepository;

    public SetWorkDaysCommandHandler(IEmployeeWorkDayRepository workDayRepository)
    {
        _workDayRepository = workDayRepository;
    }

    public async Task<OperationResult> Handle(SetWorkDaysCommand request, CancellationToken cancellationToken)
    {
        if (!DateOnly.TryParse(request.Dto.From, out var from) ||
            !DateOnly.TryParse(request.Dto.To, out var to))
            return OperationResult.Failure("Ogiltigt datumformat. Använd yyyy-MM-dd.");

        if (to < from)
            return OperationResult.Failure("Slutdatum måste vara efter startdatum.");

        if ((to.DayNumber - from.DayNumber) > 90)
            return OperationResult.Failure("Max 90 dagar per anrop.");

        var workDays = new List<EmployeeWorkDayModel>();
        foreach (var dto in request.Dto.WorkDays)
        {
            if (!DateOnly.TryParse(dto.Date, out var date))
                return OperationResult.Failure($"Ogiltigt datum: {dto.Date}");

            if (date < from || date > to)
                return OperationResult.Failure($"Datumet {dto.Date} är utanför det angivna intervallet.");

            if (!TimeSpan.TryParse(dto.StartTime, out var start) ||
                !TimeSpan.TryParse(dto.EndTime, out var end))
                return OperationResult.Failure($"Ogiltig tid för {dto.Date}.");

            if (end <= start)
                return OperationResult.Failure($"Sluttid måste vara efter starttid för {dto.Date}.");

            workDays.Add(new EmployeeWorkDayModel
            {
                Id = Guid.NewGuid(),
                EmployeeId = request.EmployeeId,
                Date = date,
                StartTime = start,
                EndTime = end,
            });
        }

        await _workDayRepository.SetWorkDaysForRangeAsync(request.EmployeeId, from, to, workDays);
        return OperationResult.Success();
    }
}
