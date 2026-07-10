using System.Net;
using Application_Layer.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Infrastructure_Layer.Notifications
{
    public sealed class PasswordResetEmailService : IPasswordResetEmailService
    {
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;

        public PasswordResetEmailService(IEmailSender emailSender, IConfiguration configuration)
        {
            _emailSender = emailSender;
            _configuration = configuration;
        }

        public async Task SendResetLinkAsync(string email, string? firstName, string resetToken, CancellationToken ct)
        {
            var appBaseUrl = (_configuration["Notifications:AppBaseUrl"] ?? "").TrimEnd('/');
            var brandName = _configuration["Notifications:BrandName"] ?? "ElsaBeauty";
            var supportEmail = _configuration["Notifications:SupportEmail"];

            var resetLink =
                $"{appBaseUrl}/reset-password" +
                $"?email={Uri.EscapeDataString(email)}" +
                $"&token={Uri.EscapeDataString(resetToken)}";

            var greeting = string.IsNullOrWhiteSpace(firstName) ? "Hej!" : $"Hej {firstName}!";
            var subject = $"Återställ ditt lösenord hos {brandName}";
            var message =
                $"{greeting}\n\n" +
                "Vi har fått en begäran om att återställa lösenordet för ditt konto. " +
                "Klicka på knappen nedan för att välja ett nytt lösenord. " +
                "Länken är giltig en begränsad tid.\n\n" +
                "Om du inte har begärt någon återställning kan du bortse från det här mejlet — " +
                "ditt lösenord förblir oförändrat.";

            var (html, plainText) = EmailTemplateBuilder.BuildActionTemplate(
                title: "Återställ ditt lösenord",
                message: message,
                actionText: "Välj nytt lösenord",
                actionUrl: resetLink,
                brandName: brandName,
                supportEmail: supportEmail,
                footerNote: $"Du får detta mejl eftersom en lösenordsåterställning begärts för ditt konto hos {WebUtility.HtmlEncode(brandName)}.");

            await _emailSender.SendAsync(email, subject, plainText, html, ct);
        }
    }
}
