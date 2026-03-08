using FluentValidation;

namespace Application_Layer.Commands.BookingCommands.CancelBooking
{
    public class CancelBookingCommandValidator : AbstractValidator<CancelBookingCommand>
    {
        public CancelBookingCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Booking ID is required.");
        }
    }
}
