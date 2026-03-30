using Application_Layer.Interfaces;
using Domain_Layer.Common;
using MediatR;

namespace Application_Layer.Commands.UserCommands.RevokeRefreshToken
{
    public class RevokeRefreshTokenCommandHandler : IRequestHandler<RevokeRefreshTokenCommand, OperationResult>
    {
        private readonly IRefreshTokenService _refreshTokenService;

        public RevokeRefreshTokenCommandHandler(IRefreshTokenService refreshTokenService)
        {
            _refreshTokenService = refreshTokenService;
        }

        public async Task<OperationResult> Handle(RevokeRefreshTokenCommand request, CancellationToken cancellationToken)
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
                return OperationResult.Success();
            }

            return OperationResult.Failure("Either a refresh token or user ID is required.");
        }
    }
}
