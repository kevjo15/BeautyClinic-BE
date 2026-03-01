using Domain_Layer.Models;

namespace Application_Layer.Interfaces
{
    public interface IRefreshTokenRepository
    {
        /// <summary>
        /// Creates a new refresh token in the database.
        /// </summary>
        Task<UserRefreshToken> CreateAsync(UserRefreshToken refreshToken);

        /// <summary>
        /// Gets a refresh token by its hash.
        /// </summary>
        Task<UserRefreshToken?> GetByTokenHashAsync(string tokenHash);

        /// <summary>
        /// Gets all active (non-revoked, non-expired) refresh tokens for a user.
        /// </summary>
        Task<List<UserRefreshToken>> GetActiveTokensByUserIdAsync(string userId);

        /// <summary>
        /// Gets all refresh tokens for a user (including revoked/expired).
        /// </summary>
        Task<List<UserRefreshToken>> GetAllTokensByUserIdAsync(string userId);

        /// <summary>
        /// Updates an existing refresh token.
        /// </summary>
        Task UpdateAsync(UserRefreshToken refreshToken);

        /// <summary>
        /// Revokes all active refresh tokens for a user.
        /// </summary>
        Task RevokeAllTokensForUserAsync(string userId, string? revokedByIp = null, string? reason = null);

        /// <summary>
        /// Revokes a specific refresh token and optionally all tokens in its chain.
        /// Used for detecting token reuse attacks.
        /// </summary>
        Task RevokeTokenChainAsync(string tokenHash, string? revokedByIp = null, string? reason = null);

        /// <summary>
        /// Deletes expired tokens older than the specified date.
        /// Used for cleanup tasks.
        /// </summary>
        Task DeleteExpiredTokensAsync(DateTime olderThan);
    }
}
