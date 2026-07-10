using FluentValidation;

namespace Application_Layer.Commands.UserCommands.GoogleLogin
{
    public class GoogleLoginCommandValidator : AbstractValidator<GoogleLoginCommand>
    {
        public GoogleLoginCommandValidator()
        {
            RuleFor(x => x.IdToken)
                .NotEmpty()
                .WithMessage("Google ID-token saknas.");
        }
    }
}
