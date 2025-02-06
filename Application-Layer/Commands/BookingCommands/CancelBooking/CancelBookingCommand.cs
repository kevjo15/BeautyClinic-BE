using MediatR;

namespace Application_Layer.Commands.BookingCommands.CancelBooking
{
    public record CancelBookingCommand(Guid Id) : IRequest<bool>;
}
