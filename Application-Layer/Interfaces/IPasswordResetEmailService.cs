namespace Application_Layer.Interfaces
{
    /// <summary>
    /// Skickar återställningslänken. Länkbygge (AppBaseUrl) och mall
    /// är infrastruktursdetaljer och hör hemma i implementationen.
    /// </summary>
    public interface IPasswordResetEmailService
    {
        Task SendResetLinkAsync(string email, string? firstName, string resetToken, CancellationToken ct);
    }
}
