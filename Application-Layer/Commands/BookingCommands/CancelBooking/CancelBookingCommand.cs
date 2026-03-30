using Domain_Layer.Common;
using MediatR;

namespace Application_Layer.Commands.BookingCommands.CancelBooking
{
    public record CancelBookingCommand(Guid Id, string RequestingUserId, bool CanManageBooking) : IRequest<OperationResult>;
}
