using Application_Layer.Validators.ValidationExtensions;
using FluentValidation;

namespace Application_Layer.Commands.UserCommands.ResendEmailConfirmation
{
    public class ResendEmailConfirmationCommandValidator : AbstractValidator<ResendEmailConfirmationCommand>
    {
        public ResendEmailConfirmationCommandValidator()
        {
            RuleFor(x => x.Request.Email)
                .MustBeValidEmail();
        }
    }
}
