namespace Application_Layer.DTOs
{
    /// <summary>
    /// Verifierade claims ur ett Google ID-token. Subject är Googles stabila
    /// användar-id (används som ProviderKey i AspNetUserLogins).
    /// </summary>
    public sealed record GoogleUserInfo(
        string Subject,
        string Email,
        bool EmailVerified,
        string? FirstName,
        string? LastName);
}
