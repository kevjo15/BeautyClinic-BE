using FluentValidation;

namespace Application_Layer.Validators.ValidationExtensions
{
    public static class CategoryNameValidationExtension
    {
        public static IRuleBuilderOptions<T, string> ValidCategoryName<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty().WithMessage("Category name is required.")
                .Length(1, 100).WithMessage("Category name must be between 1 and 100 characters.");
        }
    }
} 