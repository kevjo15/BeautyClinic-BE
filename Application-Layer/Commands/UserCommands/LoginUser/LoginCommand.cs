using Application_Layer.DTO_s;
using Application_Layer.DTOs;
using Domain_Layer.Common;
using MediatR;

namespace Application_Layer.Commands.UserCommands.Login
{
    public class LoginCommand : IRequest<OperationResult<AuthTokenPairDTO>>
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
