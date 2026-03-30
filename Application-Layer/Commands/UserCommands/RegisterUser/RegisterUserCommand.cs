using Application_Layer.DTOs;
using Domain_Layer.Common;
using Domain_Layer.Models;
using MediatR;

namespace Application_Layer.Commands.UserCommands.RegisterUser
{
    public class RegisterUserCommand : IRequest<OperationResult<UserModel>>
    {
        public RegisterUserDTO NewUser { get; set; }

        public RegisterUserCommand(RegisterUserDTO newUser)
        {
            NewUser = newUser;
        }
    }
}
