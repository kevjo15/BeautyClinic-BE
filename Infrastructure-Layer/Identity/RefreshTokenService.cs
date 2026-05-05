using System.Security.Cryptography;
using System.Text;
using Application_Layer.DTOs;
using Application_Layer.Interfaces;
using Domain_Layer.Common;
using Domain_Layer.Models;
using Microsoft.Extensions.Configuration;

namespace Infrastructure_Layer.Identity;

public sealed class RefreshTokenService : IRefreshTokenService
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
        _pepper = configuration["JwtSettings:RefreshTokenPepper"] ?? "ElsaBeautyDefaultPepper2024";
    }

    public async Task<(string RawToken, UserRefreshToken TokenEntity)> GenerateRefreshTokenAsync(
        string userId,
        string? ipAddress = null,
        string? userAgent = null)
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        var rawToken = Convert.ToBase64String(randomBytes);

        var tokenHash = HashToken(rawToken);
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

    public async Task<OperationResult<UserRefreshToken>> ValidateRefreshTokenAsync(string rawToken)
    {
        if (string.IsNullOrWhiteSpace(rawToken))
        {
            return OperationResult<UserRefreshToken>.Failure("Refresh token is required.");
        }

        var tokenHash = HashToken(rawToken);
        var token = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash);

        if (token == null)
        {
            return OperationResult<UserRefreshToken>.Failure("Invalid refresh token.");
        }

        if (token.IsRevoked)
        {
            await _refreshTokenRepository.RevokeTokenChainAsync(
                tokenHash,
                reason: "Token reuse detected - potential token theft");

            return OperationResult<UserRefreshToken>.Failure(
                "Token has been revoked. All sessions have been terminated for security.");
        }

        if (token.IsExpired)
        {
            return OperationResult<UserRefreshToken>.Failure("Refresh token has expired.");
        }

        return OperationResult<UserRefreshToken>.Success(token);
    }

    public async Task<OperationResult<RefreshTokenRotationResultDTO>> RotateRefreshTokenAsync(
        string oldRawToken,
        string? ipAddress = null,
        string? userAgent = null)
    {
        var validationResult = await ValidateRefreshTokenAsync(oldRawToken);
        if (!validationResult.Successful || validationResult.Data == null)
        {
            return OperationResult<RefreshTokenRotationResultDTO>.Failure(
                validationResult.Error ?? "Invalid or expired refresh token.");
        }

        var oldToken = validationResult.Data;

        var (newRawToken, newTokenEntity) = await GenerateRefreshTokenAsync(
            oldToken.UserId,
            ipAddress,
            userAgent);

        oldToken.RevokedAt = DateTime.UtcNow;
        oldToken.RevokedByIp = ipAddress;
        oldToken.ReplacedByTokenHash = newTokenEntity.TokenHash;
        oldToken.RevokedReason = "Rotated - replaced by new token";

        await _refreshTokenRepository.UpdateAsync(oldToken);
        return OperationResult<RefreshTokenRotationResultDTO>.Success(
            new RefreshTokenRotationResultDTO(newRawToken, newTokenEntity));
    }

    public async Task<OperationResult> RevokeRefreshTokenAsync(
        string rawToken,
        string? ipAddress = null,
        string? reason = null)
    {
        if (string.IsNullOrWhiteSpace(rawToken))
        {
            return OperationResult.Failure("Refresh token is required.");
        }

        var tokenHash = HashToken(rawToken);
        var token = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash);

        if (token == null || token.IsRevoked)
        {
            return OperationResult.Failure("Refresh token not found or already revoked.");
        }

        token.RevokedAt = DateTime.UtcNow;
        token.RevokedByIp = ipAddress;
        token.RevokedReason = reason ?? "User logout";

        await _refreshTokenRepository.UpdateAsync(token);
        return OperationResult.Success();
    }

    public Task RevokeAllUserTokensAsync(
        string userId,
        string? ipAddress = null,
        string? reason = null)
    {
        return _refreshTokenRepository.RevokeAllTokensForUserAsync(
            userId,
            ipAddress,
            reason ?? "All tokens revoked");
    }

    public string HashToken(string rawToken)
    {
        var tokenWithPepper = rawToken + _pepper;

        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(tokenWithPepper);
        var hashBytes = sha256.ComputeHash(bytes);

        return Convert.ToBase64String(hashBytes);
    }
}
