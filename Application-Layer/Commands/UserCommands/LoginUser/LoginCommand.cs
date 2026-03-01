using Application_Layer.DTO_s;
using MediatR;

namespace Application_Layer.Commands.UserCommands.Login
{
    public class LoginCommand : IRequest<LoginResult>
    {
        public LoginUserDTO LoginUserDTO { get; }
        public string? IpAddress { get; }
        public string? UserAgent { get; }

        public LoginCommand(LoginUserDTO loginUserDTO, string? ipAddress = null, string? userAgent = null)
        {
            LoginUserDTO = loginUserDTO;
            IpAddress = ipAddress;
            UserAgent = userAgent;
        }
    }
}
