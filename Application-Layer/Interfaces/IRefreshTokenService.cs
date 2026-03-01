using Domain_Layer.Models;

namespace Application_Layer.Interfaces
{
    public interface IRefreshTokenService
    {
        /// <summary>
        /// Generates a new refresh token for the user.
        /// Returns the raw token (to be sent to client) and the token entity (with hashed token for DB).
        /// </summary>
        Task<(string RawToken, UserRefreshToken TokenEntity)> GenerateRefreshTokenAsync(
            string userId,
            string? ipAddress = null,
            string? userAgent = null);

        /// <summary>
        /// Validates a refresh token and returns the user if valid.
        /// </summary>
        Task<(bool IsValid, UserRefreshToken? Token, string? Error)> ValidateRefreshTokenAsync(string rawToken);

        /// <summary>
        /// Rotates a refresh token: revokes the old one and creates a new one.
        /// Returns the new raw token and token entity.
        /// </summary>
        Task<(string RawToken, UserRefreshToken TokenEntity)?> RotateRefreshTokenAsync(
            string oldRawToken,
            string? ipAddress = null,
            string? userAgent = null);

        /// <summary>
        /// Revokes a refresh token.
        /// </summary>
        Task<bool> RevokeRefreshTokenAsync(
            string rawToken,
            string? ipAddress = null,
            string? reason = null);

        /// <summary>
        /// Revokes all refresh tokens for a user.
        /// </summary>
        Task RevokeAllUserTokensAsync(
            string userId,
            string? ipAddress = null,
            string? reason = null);

        /// <summary>
        /// Hashes a raw token using SHA256.
        /// </summary>
        string HashToken(string rawToken);
    }
}
