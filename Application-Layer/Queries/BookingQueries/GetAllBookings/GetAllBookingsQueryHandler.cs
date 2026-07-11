using MediatR;
using Application_Layer.DTOs;
using Application_Layer.Interfaces;
using Application_Layer.Mapping;

namespace Application_Layer.Queries.BookingQueries.GetAllBookings
{
    public class GetAllBookingsQueryHandler : IRequestHandler<GetAllBookingsQuery, List<BookingDTO>>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IApplicationMapper _mapper;

        public GetAllBookingsQueryHandler(IBookingRepository bookingRepository, IApplicationMapper mapper)
        {
            _bookingRepository = bookingRepository;
            _mapper = mapper;
        }

        public async Task<List<BookingDTO>> Handle(GetAllBookingsQuery request, CancellationToken cancellationToken)
        {
            var bookings = await _bookingRepository.GetAllAsync();
            return _mapper.ToBookingDtoList(bookings);
        }
    }
}
