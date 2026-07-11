using Application_Layer.DTOs;
using MediatR;

namespace Application_Layer.Queries.BookingQueries.GetBookingsReport
{
    /// <summary>Bokningsrapport för adminvyn. From/To är inklusiva datum.</summary>
    public record GetBookingsReportQuery(DateTime From, DateTime To) : IRequest<BookingsReportDTO>;
}
