using MediatR;
using Application_Layer.DTOs;

namespace Application_Layer.Queries.BookingQueries.GetAllBookings
{
    public record GetAllBookingsQuery : IRequest<List<BookingDTO>>;
}
