using Application_Layer.Interfaces;
using MediatR;

namespace Application_Layer.Commands.UserCommands.RevokeRefreshToken
{
    public class RevokeRefreshTokenCommandHandler : IRequestHandler<RevokeRefreshTokenCommand, bool>
    {
        private readonly IRefreshTokenService _refreshTokenService;

        public RevokeRefreshTokenCommandHandler(IRefreshTokenService refreshTokenService)
        {
            _refreshTokenService = refreshTokenService;
        }

        public async Task<bool> Handle(RevokeRefreshTokenCommand request, CancellationToken cancellationToken)
        {
            // If a specific refresh token is provided, revoke just that token
            if (!string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return await _refreshTokenService.RevokeRefreshTokenAsync(
                    request.RefreshToken,
                    request.IpAddress,
                    request.Reason ?? "User logout");
            }

            // If only userId is provided, revoke all tokens for that user
            if (!string.IsNullOrWhiteSpace(request.UserId))
            {
                await _refreshTokenService.RevokeAllUserTokensAsync(
                    request.UserId,
                    request.IpAddress,
                    request.Reason ?? "All sessions terminated");
                return true;
            }

            return false;
        }
    }
}