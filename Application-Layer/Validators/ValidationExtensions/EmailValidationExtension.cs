using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application_Layer.Validators.ValidationExtensions
{
    public static class EmailValidationExtension
    {
        public static IRuleBuilderOptions<T, string> MustBeValidEmail<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty().WithMessage("Email is required.")
                .NotNull().WithMessage("Email can't be null")
                .EmailAddress().WithMessage("A valid email address is required.");
        }
    }
}
