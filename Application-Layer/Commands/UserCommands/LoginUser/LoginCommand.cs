using Application_Layer.DTO_s;
using MediatR;

namespace Application_Layer.Commands.UserCommands.Login
{
    public class LoginCommand : IRequest<LoginResult>
    {
        public LoginUserDTO LoginUserDTO { get; private set; }
        public LoginCommand(LoginUserDTO loginUserDTO)
        {
            LoginUserDTO = loginUserDTO;
        }
    }
}
