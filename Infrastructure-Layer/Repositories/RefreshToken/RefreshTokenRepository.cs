using Application_Layer.Interfaces;
using Domain_Layer.Models;
using Infrastructure_Layer.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure_Layer.Repositories.RefreshToken
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly ElsaBeautyDbContext _context;

        public RefreshTokenRepository(ElsaBeautyDbContext context)
        {
            _context = context;
        }

        public async Task<UserRefreshToken> CreateAsync(UserRefreshToken refreshToken)
        {
            await _context.UserRefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();
            return refreshToken;
        }

        public async Task<UserRefreshToken?> GetByTokenHashAsync(string tokenHash)
        {
            return await _context.UserRefreshTokens
                .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash);
        }

        public async Task<List<UserRefreshToken>> GetActiveTokensByUserIdAsync(string userId)
        {
            return await _context.UserRefreshTokens
                .Where(rt => rt.UserId == userId && rt.RevokedAt == null && rt.ExpiresAt > DateTime.UtcNow)
                .ToListAsync();
        }

        public async Task<List<UserRefreshToken>> GetAllTokensByUserIdAsync(string userId)
        {
            return await _context.UserRefreshTokens
                .Where(rt => rt.UserId == userId)
                .OrderByDescending(rt => rt.CreatedAt)
                .ToListAsync();
        }

        public async Task UpdateAsync(UserRefreshToken refreshToken)
        {
            _context.UserRefreshTokens.Update(refreshToken);
            await _context.SaveChangesAsync();
        }

        public async Task RevokeAllTokensForUserAsync(string userId, string? revokedByIp = null, string? reason = null)
        {
            var activeTokens = await GetActiveTokensByUserIdAsync(userId);
            var now = DateTime.UtcNow;

            foreach (var token in activeTokens)
            {
                token.RevokedAt = now;
                token.RevokedByIp = revokedByIp;
                token.RevokedReason = reason ?? "User logout - all tokens revoked";
            }

            await _context.SaveChangesAsync();
        }

        public async Task RevokeTokenChainAsync(string tokenHash, string? revokedByIp = null, string? reason = null)
        {
            var token = await GetByTokenHashAsync(tokenHash);
            if (token == null) return;

            var now = DateTime.UtcNow;
            var userId = token.UserId;

            // Revoke all active tokens for this user (security measure for token reuse)
            var allUserTokens = await _context.UserRefreshTokens
                .Where(rt => rt.UserId == userId && rt.RevokedAt == null)
                .ToListAsync();

            foreach (var t in allUserTokens)
            {
                t.RevokedAt = now;
                t.RevokedByIp = revokedByIp;
                t.RevokedReason = reason ?? "Token reuse detected - chain invalidated";
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteExpiredTokensAsync(DateTime olderThan)
        {
            var expiredTokens = await _context.UserRefreshTokens
                .Where(rt => rt.ExpiresAt < olderThan || (rt.RevokedAt != null && rt.RevokedAt < olderThan))
                .ToListAsync();

            _context.UserRefreshTokens.RemoveRange(expiredTokens);
            await _context.SaveChangesAsync();
        }
    }
}
