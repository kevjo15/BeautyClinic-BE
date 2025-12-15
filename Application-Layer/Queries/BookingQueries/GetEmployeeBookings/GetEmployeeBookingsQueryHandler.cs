using Application_Layer.DTOs;
using Application_Layer.Interfaces;
using AutoMapper;
using MediatR;

namespace Application_Layer.Queries.BookingQueries.GetEmployeeBookings
{
    public class GetEmployeeBookingsQueryHandler : IRequestHandler<GetEmployeeBookingsQuery, List<BookingDTO>>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IMapper _mapper;

        public GetEmployeeBookingsQueryHandler(IBookingRepository bookingRepository, IMapper mapper)
        {
            _bookingRepository = bookingRepository;
            _mapper = mapper;
        }

        public async Task<List<BookingDTO>> Handle(GetEmployeeBookingsQuery request, CancellationToken cancellationToken)
        {
            var bookings = await _bookingRepository.GetByEmployeeAndRangeAsync(request.EmployeeId, request.From, request.To);
            return _mapper.Map<List<BookingDTO>>(bookings);
        }
    }
}
