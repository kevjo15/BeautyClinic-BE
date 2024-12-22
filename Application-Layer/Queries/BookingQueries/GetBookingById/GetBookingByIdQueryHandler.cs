using MediatR;
using Application_Layer.DTOs;
using Application_Layer.Interfaces;
using AutoMapper;
using Domain_Layer.Models;

namespace Application_Layer.Queries.BookingQueries.GetBookingById
{
    public class GetBookingByIdQueryHandler : IRequestHandler<GetBookingByIdQuery, BookingDTO>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IMapper _mapper;

        public GetBookingByIdQueryHandler(IBookingRepository bookingRepository, IMapper mapper)
        {
            _bookingRepository = bookingRepository;
            _mapper = mapper;
        }

        public async Task<BookingDTO> Handle(GetBookingByIdQuery request, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepository.GetByIdAsync(request.Id);
            
            if (booking == null)
                throw new KeyNotFoundException($"Booking with ID {request.Id} was not found.");

            return _mapper.Map<BookingDTO>(booking);
        }
    }
}
