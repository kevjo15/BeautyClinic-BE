using System.Security.Cryptography;
using System.Text;
using Application_Layer.Interfaces;
using Domain_Layer.Models;
using Microsoft.Extensions.Configuration;

namespace Application_Layer.Services
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IConfiguration _configuration;
        private readonly string _pepper;

        public RefreshTokenService(
            IRefreshTokenRepository refreshTokenRepository,
            IConfiguration configuration)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _configuration = configuration;

            // Server-side pepper for additional security (optional)
            _pepper = configuration["JwtSettings:RefreshTokenPepper"] ?? "ElsaBeautyDefaultPepper2024";
        }

        public async Task<(string RawToken, UserRefreshToken TokenEntity)> GenerateRefreshTokenAsync(
            string userId,
            string? ipAddress = null,
            string? userAgent = null)
        {
            // Generate a cryptographically secure random token
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            var rawToken = Convert.ToBase64String(randomBytes);

            // Hash the token for storage
            var tokenHash = HashToken(rawToken);

            // Get refresh token expiry from config (default 7 days)
            var refreshTokenDays = _configuration.GetValue<int>("JwtSettings:RefreshTokenExpiryDays", 7);

            var tokenEntity = new UserRefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                TokenHash = tokenHash,
                ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenDays),
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = ipAddress,
                UserAgent = userAgent
            };

            await _refreshTokenRepository.CreateAsync(tokenEntity);

            return (rawToken, tokenEntity);
        }

        public async Task<(bool IsValid, UserRefreshToken? Token, string? Error)> ValidateRefreshTokenAsync(string rawToken)
        {
            if (string.IsNullOrWhiteSpace(rawToken))
            {
                return (false, null, "Refresh token is required.");
            }

            var tokenHash = HashToken(rawToken);
            var token = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash);

            if (token == null)
            {
                return (false, null, "Invalid refresh token.");
            }

            // Check if token has been revoked
            if (token.IsRevoked)
            {
                // Token reuse detected! This is a potential attack.
                // Revoke all tokens in the family/chain for this user
                await _refreshTokenRepository.RevokeTokenChainAsync(
                    tokenHash,
                    reason: "Token reuse detected - potential token theft");

                return (false, null, "Token has been revoked. All sessions have been terminated for security.");
            }

            // Check if token has expired
            if (token.IsExpired)
            {
                return (false, null, "Refresh token has expired.");
            }

            return (true, token, null);
        }

        public async Task<(string RawToken, UserRefreshToken TokenEntity)?> RotateRefreshTokenAsync(
            string oldRawToken,
            string? ipAddress = null,
            string? userAgent = null)
        {
            var (isValid, oldToken, error) = await ValidateRefreshTokenAsync(oldRawToken);

            if (!isValid || oldToken == null)
            {
                return null;
            }

            // Generate new refresh token
            var (newRawToken, newTokenEntity) = await GenerateRefreshTokenAsync(
                oldToken.UserId,
                ipAddress,
                userAgent);

            // Mark old token as revoked and set the replacement
            oldToken.RevokedAt = DateTime.UtcNow;
            oldToken.RevokedByIp = ipAddress;
            oldToken.ReplacedByTokenHash = newTokenEntity.TokenHash;
            oldToken.RevokedReason = "Rotated - replaced by new token";

            await _refreshTokenRepository.UpdateAsync(oldToken);

            return (newRawToken, newTokenEntity);
        }

        public async Task<bool> RevokeRefreshTokenAsync(
            string rawToken,
            string? ipAddress = null,
            string? reason = null)
        {
            if (string.IsNullOrWhiteSpace(rawToken))
            {
                return false;
            }

            var tokenHash = HashToken(rawToken);
            var token = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash);

            if (token == null || token.IsRevoked)
            {
                return false;
            }

            token.RevokedAt = DateTime.UtcNow;
            token.RevokedByIp = ipAddress;
            token.RevokedReason = reason ?? "User logout";

            await _refreshTokenRepository.UpdateAsync(token);

            return true;
        }

        public async Task RevokeAllUserTokensAsync(
            string userId,
            string? ipAddress = null,
            string? reason = null)
        {
            await _refreshTokenRepository.RevokeAllTokensForUserAsync(
                userId,
                ipAddress,
                reason ?? "All tokens revoked");
        }

        public string HashToken(string rawToken)
        {
            // Combine token with pepper for additional security
            var tokenWithPepper = rawToken + _pepper;

            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(tokenWithPepper);
            var hashBytes = sha256.ComputeHash(bytes);

            return Convert.ToBase64String(hashBytes);
        }
    }
}
