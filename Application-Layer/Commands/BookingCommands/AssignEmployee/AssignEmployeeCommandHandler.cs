using Application_Layer.Interfaces;
using Application_Layer.DTOs;
using Application_Layer.Mapping;
using Domain_Layer.Common;
using Domain_Layer.Models;
using MediatR;

namespace Application_Layer.Commands.BookingCommands.AssignEmployee
{
    public class AssignEmployeeCommandHandler : IRequestHandler<AssignEmployeeCommand, OperationResult<BookingDTO>>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IConversationRepository _conversationRepository;
        private readonly IApplicationMapper _mapper;

        public AssignEmployeeCommandHandler(
            IBookingRepository bookingRepository,
            IConversationRepository conversationRepository,
            IApplicationMapper mapper)
        {
            _bookingRepository = bookingRepository;
            _conversationRepository = conversationRepository;
            _mapper = mapper;
        }

        public async Task<OperationResult<BookingDTO>> Handle(AssignEmployeeCommand request, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId);
            if (booking == null)
            {
                return OperationResult<BookingDTO>.Failure($"Booking {request.BookingId} not found.");
            }

            var hasConflict = await _bookingRepository.HasConflictAsync(
                booking.Id, request.EmployeeId, booking.StartTime, booking.EndTime);
            if (hasConflict)
                return OperationResult<BookingDTO>.Failure("Medarbetaren har redan en bokning på den valda tiden.");

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
            return OperationResult<BookingDTO>.Success(_mapper.ToBookingDto(booking));
        }
    }
}
