using MediatR;
using Application_Layer.DTOs;

namespace Application_Layer.Queries.BookingQueries.GetBookingById
{
    public record GetBookingByIdQuery(Guid Id) : IRequest<BookingDTO>;
}
