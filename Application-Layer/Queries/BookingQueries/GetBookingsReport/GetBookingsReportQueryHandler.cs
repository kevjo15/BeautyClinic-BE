using Application_Layer.DTOs;
using Application_Layer.Interfaces;
using Domain_Layer.Models;
using MediatR;

namespace Application_Layer.Queries.BookingQueries.GetBookingsReport
{
    public class GetBookingsReportQueryHandler : IRequestHandler<GetBookingsReportQuery, BookingsReportDTO>
    {
        private readonly IBookingRepository _bookingRepository;

        public GetBookingsReportQueryHandler(IBookingRepository bookingRepository)
        {
            _bookingRepository = bookingRepository;
        }

        public async Task<BookingsReportDTO> Handle(GetBookingsReportQuery request, CancellationToken cancellationToken)
        {
            var from = request.From.Date;
            var toExclusive = request.To.Date.AddDays(1);

            // Ta med avbokade (soft-deletade) rader så rapporten kan visa dem.
            var bookings = await _bookingRepository.GetByDateRangeAsync(from, toExclusive, includeCancelled: true);

            var rows = bookings
                .OrderBy(b => b.StartTime)
                .Select(b => new BookingsReportRowDTO
                {
                    StartTime = b.StartTime,
                    ServiceName = b.Service?.Name ?? "Okänd behandling",
                    Price = b.Service?.Price ?? 0m,
                    CustomerName = FormatName(b.User?.FirstName, b.User?.LastName),
                    EmployeeName = FormatName(b.Employee?.FirstName, b.Employee?.LastName),
                    IsCancelled = b.Status == BookingStatus.Cancelled,
                })
                .ToList();

            // Omsättning och antal räknas bara på aktiva bokningar — avbokade
            // gav ingen intäkt och ska inte blåsa upp siffrorna.
            var activeRows = rows.Where(r => !r.IsCancelled).ToList();

            var perService = activeRows
                .GroupBy(r => r.ServiceName)
                .Select(g => new BookingsReportServiceLineDTO
                {
                    ServiceName = g.Key,
                    Count = g.Count(),
                    Revenue = g.Sum(r => r.Price),
                })
                .OrderByDescending(l => l.Revenue)
                .ToList();

            return new BookingsReportDTO
            {
                From = from,
                To = request.To.Date,
                TotalBookings = activeRows.Count,
                TotalRevenue = activeRows.Sum(r => r.Price),
                CancelledBookings = rows.Count - activeRows.Count,
                PerService = perService,
                Rows = rows,
            };
        }

        private static string FormatName(string? firstName, string? lastName)
        {
            var name = $"{firstName} {lastName}".Trim();
            return string.IsNullOrWhiteSpace(name) ? "–" : name;
        }
    }
}
