using Domain_Layer.Common;
using MediatR;

namespace Application_Layer.Commands.BookingCommands.MarkBookingNoShow
{
    /// <summary>
    /// Personal markerar en passerad bokning som utebliven → drar no-show-avgiften
    /// off-session från kundens sparade kort och sätter status NoShow.
    /// </summary>
    public sealed record MarkBookingNoShowCommand(Guid BookingId) : IRequest<OperationResult>;
}
