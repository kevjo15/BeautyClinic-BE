using FluentValidation;

namespace Application_Layer.Commands.UserCommands.ConfirmEmail
{
    public class ConfirmEmailCommandValidator : AbstractValidator<ConfirmEmailCommand>
    {
        public ConfirmEmailCommandValidator()
        {
            RuleFor(x => x.Request.UserId)
                .NotEmpty().WithMessage("User id is required.");

            RuleFor(x => x.Request.Token)
                .NotEmpty().WithMessage("Token is required.");
        }
    }
}
