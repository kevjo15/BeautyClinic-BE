using Application_Layer.Validators.ValidationExtensions;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
