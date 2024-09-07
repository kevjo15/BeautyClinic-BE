using Application_Layer.Validators.ValidationExtensions;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application_Layer.Commands.UserCommands.Login
{
    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator() 
        {
            RuleFor(x => x.LoginUserDTO.Email)
                .MustBeValidEmail();

            RuleFor(x => x.LoginUserDTO.Password)
                .MustBeValidPassword();
        }
    }
}
