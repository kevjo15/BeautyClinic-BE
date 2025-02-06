using FluentValidation;

namespace Application_Layer.Validators.ValidationExtensions
{
    public static class DescriptionValidationExtension
    {
        public static IRuleBuilderOptions<T, string> MustBeValidDescription<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .MaximumLength(500).WithMessage("Description must be less than 500 characters.");
        }
    }
} 