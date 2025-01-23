using Application_Layer.Validators.ValidationExtensions;
using FluentValidation;

namespace Application_Layer.Commands.UserCommands.RegisterUser
{
    public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
    {
        public RegisterUserCommandValidator()
        {
            RuleFor(user => user.NewUser.UserName)
                .MustBeValidUserName();

            RuleFor(user => user.NewUser.FirstName)
                .MustBeValidName();

            RuleFor(user => user.NewUser.LastName)
                .MustBeValidName();

            RuleFor(user => user.NewUser.Email)
                .MustBeValidEmail();

            RuleFor(user => user.NewUser.Password)
                .MustBeValidPassword();

            RuleFor(user => user.NewUser.ConfirmPassword)
                .Equal(user => user.NewUser.Password).WithMessage("Passwords do not match.");
        }
    }
}
