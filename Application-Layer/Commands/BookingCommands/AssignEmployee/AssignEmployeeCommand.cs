using Application_Layer.DTOs;
using Domain_Layer.Common;
using MediatR;

namespace Application_Layer.Commands.BookingCommands.AssignEmployee
{
    public class AssignEmployeeCommand : IRequest<OperationResult<BookingDTO>>
    {
        public Guid BookingId { get; set; }
        public string EmployeeId { get; set; } = string.Empty;
    }
}
