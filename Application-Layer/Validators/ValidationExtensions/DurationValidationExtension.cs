using FluentValidation;
using System;

namespace Application_Layer.Validators.ValidationExtensions
{
    public static class DurationValidationExtension
    {
        public static IRuleBuilderOptions<T, TimeSpan> MustBeValidDuration<T>(this IRuleBuilder<T, TimeSpan> ruleBuilder)
        {
            return ruleBuilder
                .GreaterThan(TimeSpan.Zero).WithMessage("Duration must be greater than zero.");
        }
    }
} 