using MediatR;
using AutoMapper;
using Application_Layer.Interfaces;

namespace Application_Layer.Commands.BookingCommands.UpdateBooking
{
    public class UpdateBookingCommandHandler : IRequestHandler<UpdateBookingCommand, bool>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IMapper _mapper;

        public UpdateBookingCommandHandler(IBookingRepository bookingRepository, IMapper mapper)
        {
            _bookingRepository = bookingRepository;
            _mapper = mapper;
        }

        public async Task<bool> Handle(UpdateBookingCommand request, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepository.GetByIdAsync(request.Id);

            if (booking == null)
                throw new KeyNotFoundException($"Booking with ID {request.Id} was not found.");

            _mapper.Map(request.Booking, booking);

            await _bookingRepository.UpdateAsync(booking);
            return true;
        }
    }
}
