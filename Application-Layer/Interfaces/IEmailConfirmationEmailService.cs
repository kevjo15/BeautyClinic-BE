namespace Application_Layer.Interfaces
{
    public interface IEmailConfirmationEmailService
    {
        Task SendConfirmationLinkAsync(
            string email,
            string? firstName,
            string userId,
            string confirmationToken,
            CancellationToken ct);
    }
}
