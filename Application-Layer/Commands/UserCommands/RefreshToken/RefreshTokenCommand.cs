using MediatR;

namespace Application_Layer.Commands.UserCommands.RefreshToken
{
    public class RefreshTokenCommand : IRequest<RefreshTokenResult>
    {
        public string RefreshToken { get; }

        public RefreshTokenCommand(string refreshToken)
        {
            RefreshToken = refreshToken;
        }
    }
} 