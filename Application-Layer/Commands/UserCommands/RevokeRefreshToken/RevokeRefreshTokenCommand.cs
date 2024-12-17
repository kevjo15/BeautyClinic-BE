using MediatR;

namespace Application_Layer.Commands.UserCommands.RevokeRefreshToken
{
    public class RevokeRefreshTokenCommand : IRequest<bool>
    {
        public string UserId { get; }

        public RevokeRefreshTokenCommand(string userId)
        {
            UserId = userId;
        }
    }
} 