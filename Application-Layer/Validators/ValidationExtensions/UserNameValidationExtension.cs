using FluentValidation;

namespace Application_Layer.Validators.ValidationExtensions
{
    public static class UserNameValidationExtension
    {
        public static IRuleBuilderOptions<T, string> MustBeValidUserName<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty().WithMessage("Username is required.")
                .NotNull().WithMessage("Username cannot be null.")
                .MinimumLength(3).WithMessage("Username must be at least 3 characters long.")
                .MaximumLength(20).WithMessage("Username cannot exceed 20 characters.")
                .Matches(@"^[a-zA-Z0-9]+$").WithMessage("Username can only contain letters and numbers.");
        }

    }
}