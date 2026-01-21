using Application_Layer.DTOs;
using MediatR;

namespace Application_Layer.Commands.BookingCommands.AssignEmployee
{
    public class AssignEmployeeCommand : IRequest<BookingDTO>
    {
        public Guid BookingId { get; set; }
        public string EmployeeId { get; set; } = string.Empty;
    }
}
