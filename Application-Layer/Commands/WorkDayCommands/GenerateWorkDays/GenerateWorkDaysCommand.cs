using Application_Layer.DTOs;
using Domain_Layer.Common;
using MediatR;

namespace Application_Layer.Commands.WorkDayCommands.GenerateWorkDays;

public class GenerateWorkDaysCommand : IRequest<OperationResult>
{
    public string EmployeeId { get; set; } = string.Empty;
    public GenerateWorkDaysDTO Dto { get; set; } = new();
}
