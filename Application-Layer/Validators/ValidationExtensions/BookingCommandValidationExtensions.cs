using FluentValidation;

namespace Application_Layer.Validators.ValidationExtensions
{
    public static class BookingCommandValidationExtensions
    {
        public static IRuleBuilderOptions<T, Guid> MustBeValidBookingId<T>(this IRuleBuilder<T, Guid> ruleBuilder)
        {
            return ruleBuilder
                .Must(id => id != Guid.Empty)
                .WithMessage("Booking ID must be a valid GUID.");
        }

        public static IRuleBuilderOptions<T, DateTime> MustBeInFuture<T>(this IRuleBuilder<T, DateTime> ruleBuilder)
        {
            return ruleBuilder
                .Must(dateTime => dateTime > DateTime.Now)
                .WithMessage("The date and time must be in the future.");
        }

        public static IRuleBuilderOptions<T, DateTime> MustBeAfter<T>(
            this IRuleBuilder<T, DateTime> ruleBuilder,
            Func<T, DateTime> startTimeFunc)
        {
            return ruleBuilder
                .Must((model, endTime) => endTime > startTimeFunc(model))
                .WithMessage("End time must be after start time.");
        }
    }
}
