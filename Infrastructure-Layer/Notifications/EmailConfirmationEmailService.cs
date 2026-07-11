using System.Net;
using Application_Layer.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Infrastructure_Layer.Notifications
{
    public sealed class EmailConfirmationEmailService : IEmailConfirmationEmailService
    {
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;

        public EmailConfirmationEmailService(IEmailSender emailSender, IConfiguration configuration)
        {
            _emailSender = emailSender;
            _configuration = configuration;
        }

        public async Task SendConfirmationLinkAsync(
            string email,
            string? firstName,
            string userId,
            string confirmationToken,
            CancellationToken ct)
        {
            var appBaseUrl = (_configuration["Notifications:AppBaseUrl"] ?? "").TrimEnd('/');
            var brandName = _configuration["Notifications:BrandName"] ?? "ElsaBeauty";
            var supportEmail = _configuration["Notifications:SupportEmail"];

            var confirmLink =
                $"{appBaseUrl}/confirm-email" +
                $"?userId={Uri.EscapeDataString(userId)}" +
                $"&token={Uri.EscapeDataString(confirmationToken)}";

            var greeting = string.IsNullOrWhiteSpace(firstName) ? "Hej!" : $"Hej {firstName}!";
            var subject = $"Bekräfta din e-postadress hos {brandName}";
            var message =
                $"{greeting}\n\n" +
                $"Välkommen till {brandName}! " +
                "Klicka på knappen nedan för att bekräfta din e-postadress och aktivera ditt konto.\n\n" +
                "Om du inte har skapat något konto hos oss kan du bortse från det här mejlet.";

            var (html, plainText) = EmailTemplateBuilder.BuildActionTemplate(
                title: "Bekräfta din e-postadress",
                message: message,
                actionText: "Bekräfta e-postadress",
                actionUrl: confirmLink,
                brandName: brandName,
                supportEmail: supportEmail,
                footerNote: $"Du får detta mejl eftersom ett konto registrerats med den här adressen hos {WebUtility.HtmlEncode(brandName)}.");

            await _emailSender.SendAsync(email, subject, plainText, html, ct);
        }
    }
}
