using MediatR;

namespace Application_Layer.Commands.UserCommands.RefreshToken
{
    public class RefreshAccessTokenCommand : IRequest<RefreshTokenResult>
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