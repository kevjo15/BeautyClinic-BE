using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application_Layer.Validators.ValidationExtensions
{
    public static class GuidIdValidationExtension
    {
        public static IRuleBuilderOptions<T, string> MustBeValidGuidId<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty().WithMessage("ID is required.")
                .NotNull().WithMessage("ID cannot be null.")
                .Matches("^[a-fA-F0-9-]{36}$").WithMessage("ID must be a valid GUID.");
        }
    }
}