using Application_Layer.Validators.ValidationExtensions;
using FluentValidation;

namespace Application_Layer.Commands.UserCommands.Login
{
    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(x => x.LoginUserDTO.Email)
                .MustBeValidEmail();

            // On login we only verify the field is provided.
            // Password complexity rules belong on registration only.
            RuleFor(x => x.LoginUserDTO.Password)
                .NotEmpty().WithMessage("Password is required.");
        }
    }
}
