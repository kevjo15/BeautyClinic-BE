using Application_Layer.Validators.ValidationExtensions;
using FluentValidation;

namespace Application_Layer.Commands.UserCommands.ResetPassword
{
    public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
    {
        public ResetPasswordCommandValidator()
        {
            RuleFor(x => x.Request.Email)
                .MustBeValidEmail();

            RuleFor(x => x.Request.Token)
                .NotEmpty().WithMessage("Token is required.");

            RuleFor(x => x.Request.NewPassword)
                .MustBeValidPassword();

            RuleFor(x => x.Request.ConfirmNewPassword)
                .Equal(x => x.Request.NewPassword).WithMessage("Passwords do not match.");
        }
    }
}
