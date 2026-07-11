using Application_Layer.Validators.ValidationExtensions;
using FluentValidation;

namespace Application_Layer.Commands.UserCommands.Update
{
    public class UpdateUserProfileCommandValidator : AbstractValidator<UpdateUserProfileCommand>
    {
        public UpdateUserProfileCommandValidator()
        {
            RuleFor(x => x.UpdatedProfileDTO.FirstName)
                .MustBeValidName();

            RuleFor(x => x.UpdatedProfileDTO.LastName)
                .MustBeValidName();

            RuleFor(x => x.UpdatedProfileDTO.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required.")
                .Matches(@"^\+?[0-9\s\-\(\)]{7,15}$").WithMessage("Phone number is not valid.");

            RuleFor(x => x.UserId)
               .MustBeValidGuidId();
        }

    }
}
