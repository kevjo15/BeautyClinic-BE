using Application_Layer.Validators.ValidationExtensions;
using FluentValidation;

namespace Application_Layer.Commands.UserCommands.ForgotPassword
{
    public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
    {
        public ForgotPasswordCommandValidator()
        {
            RuleFor(x => x.Request.Email)
                .MustBeValidEmail();
        }
    }
}
