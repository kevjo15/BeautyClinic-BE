using MediatR;

namespace Application_Layer.Commands.UserCommands.RefreshToken
{
    public class RefreshAccessTokenCommand : IRequest<RefreshTokenResult>
    {
        public string AccessToken { get; }

        public RefreshAccessTokenCommand(string accessToken)
        {
            AccessToken = accessToken;
        }
    }
}