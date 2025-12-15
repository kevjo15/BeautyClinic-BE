using Application_Layer.Interfaces;
using Application_Layer.DTOs;
using AutoMapper;
using Domain_Layer.Models;
using MediatR;

namespace Application_Layer.Commands.BookingCommands.AssignEmployee
{
    public class AssignEmployeeCommandHandler : IRequestHandler<AssignEmployeeCommand, BookingDTO>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IConversationRepository _conversationRepository;
        private readonly IMapper _mapper;

        public AssignEmployeeCommandHandler(
            IBookingRepository bookingRepository,
            IConversationRepository conversationRepository,
            IMapper mapper)
        {
            _bookingRepository = bookingRepository;
            _conversationRepository = conversationRepository;
            _mapper = mapper;
        }

        public async Task<BookingDTO> Handle(AssignEmployeeCommand request, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId);
            if (booking == null)
            {
                throw new KeyNotFoundException($"Booking {request.BookingId} not found.");
            }

            booking.EmployeeId = request.EmployeeId;

            // Create conversation if it does not exist
            if (!booking.ConversationId.HasValue)
            {
                var participantIds = new List<Guid>();
                if (Guid.TryParse(booking.UserId, out var patientGuid))
                {
                    participantIds.Add(patientGuid);
                }
                if (Guid.TryParse(request.EmployeeId, out var employeeGuid))
                {
                    participantIds.Add(employeeGuid);
                }

                var conversation = new ConversationModel
                {
                    Id = Guid.NewGuid(),
                    ParticipantIds = participantIds,
                    CreatedAt = DateTime.UtcNow
                };

                await _conversationRepository.CreateAsync(conversation);
                booking.ConversationId = conversation.Id;
            }

            await _bookingRepository.UpdateAsync(booking);
            return _mapper.Map<BookingDTO>(booking);
        }
    }
}
