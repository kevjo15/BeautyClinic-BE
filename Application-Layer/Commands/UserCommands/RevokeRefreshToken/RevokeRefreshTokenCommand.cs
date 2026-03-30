using Domain_Layer.Common;
using MediatR;

namespace Application_Layer.Commands.UserCommands.RevokeRefreshToken
{
    public class RevokeRefreshTokenCommand : IRequest<OperationResult>
    {
        /// <summary>
        /// The refresh token to revoke (from cookie).
        /// If provided, only this specific token will be revoked.
        /// </summary>
        public string? RefreshToken { get; }

        /// <summary>
        /// The user ID. If RefreshToken is null, all tokens for this user will be revoked.
        /// </summary>
        public string? UserId { get; }

        /// <summary>
        /// IP address of the client making the request.
        /// </summary>
        public string? IpAddress { get; }

        /// <summary>
        /// Reason for revocation.
        /// </summary>
        public string? Reason { get; }

        public RevokeRefreshTokenCommand(
            string? refreshToken = null,
            string? userId = null,
            string? ipAddress = null,
            string? reason = null)
        {
            RefreshToken = refreshToken;
            UserId = userId;
            IpAddress = ipAddress;
            Reason = reason;
        }
    }
} 
