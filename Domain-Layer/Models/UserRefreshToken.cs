namespace Domain_Layer.Models
{
    /// <summary>
    /// Represents a refresh token for JWT authentication with rotation support.
    /// Tokens are stored as SHA256 hashes for security.
    /// </summary>
    public class UserRefreshToken
    {
        public Guid Id { get; set; }

        /// <summary>
        /// The user this refresh token belongs to.
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// SHA256 hash of the refresh token. The actual token is never stored.
        /// </summary>
        public string TokenHash { get; set; } = string.Empty;

        /// <summary>
        /// When this refresh token expires.
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// When this refresh token was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// When this refresh token was revoked. Null if still active.
        /// </summary>
        public DateTime? RevokedAt { get; set; }

        /// <summary>
        /// The token hash that replaced this one during rotation.
        /// Used for detecting token reuse attacks.
        /// </summary>
        public string? ReplacedByTokenHash { get; set; }

        /// <summary>
        /// IP address that created this token.
        /// </summary>
        public string? CreatedByIp { get; set; }

        /// <summary>
        /// IP address that revoked this token.
        /// </summary>
        public string? RevokedByIp { get; set; }

        /// <summary>
        /// User agent string from the client that created this token.
        /// </summary>
        public string? UserAgent { get; set; }

        /// <summary>
        /// Reason for revocation if manually revoked.
        /// </summary>
        public string? RevokedReason { get; set; }

        // Navigation property
        public UserModel? User { get; set; }

        // Computed properties
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
        public bool IsRevoked => RevokedAt != null;
        public bool IsActive => !IsRevoked && !IsExpired;
    }
}
