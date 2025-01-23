using FluentValidation;

namespace Application_Layer.Validators.ValidationExtensions
{
    public static class ServiceIdValidationExtension
    {
        public static IRuleBuilderOptions<T, string> MustBeValidServiceId<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty().WithMessage("Service ID is required.")
                .NotNull().WithMessage("Service ID cannot be null.")
                .Matches("^[a-fA-F0-9-]{36}$").WithMessage("Service ID must be a valid GUID.");
        }
    }
}
