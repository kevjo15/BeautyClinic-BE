using FluentValidation;
using Application_Layer.Validators.ValidationExtensions;

namespace Application_Layer.Commands.BookingCommands.UpdateBooking
{
    public class UpdateBookingCommandValidator : AbstractValidator<UpdateBookingCommand>
    {
        public UpdateBookingCommandValidator()
        {
            RuleFor(command => command.Booking.StartTime)
                .MustBeInFuture()
                .WithMessage("Start time must be in the future.");

            RuleFor(command => command.Booking.EndTime)
                .MustBeInFuture()
                .WithMessage("End time must be in the future.")
                .Must((model, endTime) => endTime > model.Booking.StartTime)
                .WithMessage("End time must be after start time.");

            RuleFor(command => command.Booking.ServiceId.ToString())
                .MustBeValidServiceId()
                .WithMessage("Service ID must be a valid GUID.");
        }
    }
}
