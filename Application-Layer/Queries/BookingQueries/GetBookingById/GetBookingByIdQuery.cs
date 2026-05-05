using MediatR;
using Application_Layer.DTOs;
using Domain_Layer.Common;

namespace Application_Layer.Queries.BookingQueries.GetBookingById
{
    public record GetBookingByIdQuery(Guid Id, string RequestingUserId, bool CanManageBooking) : IRequest<OperationResult<BookingDTO>>;
}
