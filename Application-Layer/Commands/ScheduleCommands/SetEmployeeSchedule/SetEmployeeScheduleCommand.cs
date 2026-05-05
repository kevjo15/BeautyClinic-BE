using Application_Layer.DTOs;
using Domain_Layer.Common;
using MediatR;

namespace Application_Layer.Commands.ScheduleCommands.SetEmployeeSchedule
{
    public class SetEmployeeScheduleCommand : IRequest<OperationResult>
    {
        public string EmployeeId { get; }
        public List<EmployeeScheduleDTO> Schedule { get; }

        public SetEmployeeScheduleCommand(string employeeId, List<EmployeeScheduleDTO> schedule)
        {
            EmployeeId = employeeId;
            Schedule = schedule;
        }
    }
}
