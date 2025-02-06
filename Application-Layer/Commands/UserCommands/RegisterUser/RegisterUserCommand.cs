using Application_Layer.DTO_s;
using MediatR;

namespace Application_Layer.Commands.UserCommands.RegisterUser
{
    public class RegisterUserCommand : IRequest<RegisterResult>
    {
        public RegisterUserDTO NewUser { get; set; }

        public RegisterUserCommand(RegisterUserDTO newUser)
        {
            NewUser = newUser;
        }
    }
}
