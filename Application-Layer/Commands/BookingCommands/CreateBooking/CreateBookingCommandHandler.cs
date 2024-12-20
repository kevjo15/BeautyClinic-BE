using MediatR;
using AutoMapper;
using Domain_Layer.Models;
using Application_Layer.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace Application_Layer.Commands.BookingCommands.CreateBooking
{
    public class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, Guid>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IMapper _mapper;

        public CreateBookingCommandHandler(IBookingRepository bookingRepository, IMapper mapper)
        {
            _bookingRepository = bookingRepository;
            _mapper = mapper;
        }

        public async Task<Guid> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
        {
            var booking = _mapper.Map<BookingModel>(request.BookingDto);
            booking.Id = Guid.NewGuid();
            booking.UserId = request.BookingDto.UserId;

            await _bookingRepository.AddAsync(booking);

            return booking.Id;
        }
    }
} 