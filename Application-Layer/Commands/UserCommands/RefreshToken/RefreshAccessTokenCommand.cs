using Application_Layer.DTOs;
using Domain_Layer.Common;
using MediatR;

namespace Application_Layer.Commands.UserCommands.RefreshToken
{
    public class RefreshAccessTokenCommand : IRequest<OperationResult<AuthTokenPairDTO>>
    {
        public string RefreshToken { get; }
        public string? IpAddress { get; }
        public string? UserAgent { get; }

        public RefreshAccessTokenCommand(string refreshToken, string? ipAddress = null, string? userAgent = null)
        {
            RefreshToken = refreshToken;
            IpAddress = ipAddress;
            UserAgent = userAgent;
        }
    }
}
