using FluentValidation;

namespace Application_Layer.Validators.ValidationExtensions
{
    public static class ServiceNameValidationExtension
    {
        public static IRuleBuilderOptions<T, string> MustBeValidServiceName<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty().WithMessage("Service name is required.")
                .NotNull().WithMessage("Service name can't be null")
                .MinimumLength(2).WithMessage("Service name must be at least 2 characters long.")
                .MaximumLength(50).WithMessage("Service name cannot exceed 50 characters.")
                .Matches("^[a-zA-Z0-9äöüÄÖÜåÅéÉ ,.()-]+$").WithMessage("Service name can only contain letters, numbers, spaces, and certain punctuation.");
        }
    }
} 