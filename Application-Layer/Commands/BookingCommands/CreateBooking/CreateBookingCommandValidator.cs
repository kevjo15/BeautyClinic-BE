using FluentValidation;
using Application_Layer.Validators.ValidationExtensions;

namespace Application_Layer.Commands.BookingCommands.CreateBooking
{
    public class CreateBookingCommandValidator : AbstractValidator<CreateBookingCommand>
    {
        public CreateBookingCommandValidator()
        {
            RuleFor(x => x.Booking.UserId)
                .MustBeValidGuidId()
                .WithMessage("User ID must be a valid GUID.");

            RuleFor(x => x.Booking.ServiceId.ToString())
                .MustBeValidServiceId()
                .WithMessage("Service ID must be a valid GUID.");

            RuleFor(x => x.Booking.StartTime)
                .MustBeInFuture()
                .WithMessage("Start time must be in the future.");

            RuleFor(x => x.Booking.EndTime)
                .MustBeInFuture()
                .WithMessage("End time must be in the future.")
                .Must((model, endTime) => endTime > model.Booking.StartTime)
                .WithMessage("End time must be after start time.");
        }
    }
}
