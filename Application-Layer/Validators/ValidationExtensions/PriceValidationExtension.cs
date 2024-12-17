using FluentValidation;

namespace Application_Layer.Validators.ValidationExtensions
{
    public static class PriceValidationExtension
    {
        public static IRuleBuilderOptions<T, decimal> MustBeValidPrice<T>(this IRuleBuilder<T, decimal> ruleBuilder)
        {
            return ruleBuilder
                .GreaterThan(0).WithMessage("Price must be greater than zero.");
        }
    }
} 