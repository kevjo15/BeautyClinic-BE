using Application_Layer.DTOs;
using MediatR;

namespace Application_Layer.Queries.BookingQueries.GetEmployeeBookings
{
    public class GetEmployeeBookingsQuery : IRequest<List<BookingDTO>>
    {
        public string EmployeeId { get; set; } = string.Empty;
        public DateTime From { get; set; }
        public DateTime To { get; set; }
    }
}
