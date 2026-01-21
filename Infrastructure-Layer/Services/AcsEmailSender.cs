using System.Threading;
using System.Threading.Tasks;
using Application_Layer.Interfaces;
using Azure;
using Azure.Communication.Email;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure_Layer.Services
{
    public class AcsEmailSender : IEmailSender
    {
        private readonly EmailClient _emailClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AcsEmailSender> _logger;

        public AcsEmailSender(
            EmailClient emailClient,
            IConfiguration configuration,
            ILogger<AcsEmailSender> logger)
        {
            _emailClient = emailClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendAsync(string to, string subject, string plainText, string html, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(to))
            {
                _logger.LogWarning("Email recipient is empty.");
                return;
            }

            var senderAddress = _configuration["CommunicationServices:Email:SenderAddress"];
            if (string.IsNullOrWhiteSpace(senderAddress))
            {
                _logger.LogWarning("CommunicationServices:Email:SenderAddress is not configured.");
                return;
            }

            var recipients = new EmailRecipients(new[] { new EmailAddress(to) });
            var content = new EmailContent(subject)
            {
                PlainText = plainText,
                Html = html
            };

            var emailMessage = new EmailMessage(senderAddress, recipients, content);
            await _emailClient.SendAsync(WaitUntil.Started, emailMessage, ct);
        }
    }
}
