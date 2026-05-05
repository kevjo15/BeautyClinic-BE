using Application_Layer.DTOs;
using Domain_Layer.Common;
using MediatR;

namespace Application_Layer.Commands.WorkDayCommands.SetWorkDays;

public class SetWorkDaysCommand : IRequest<OperationResult>
{
    public string EmployeeId { get; set; } = string.Empty;
    public SetWorkDaysDTO Dto { get; set; } = new();
}
