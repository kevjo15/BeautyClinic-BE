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

            RuleFor(x => x.UpdatedProfileDTO.UserName)
                .MustBeValidUserName();

            RuleFor(x => x.UserId)
               .MustBeValidGuidId();
        }

    }
}
