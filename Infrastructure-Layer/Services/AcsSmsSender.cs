using System.Threading;
using System.Threading.Tasks;
using Application_Layer.Interfaces;
using Azure.Communication.Sms;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure_Layer.Services
{
    public class AcsSmsSender : ISmsSender
    {
        private readonly SmsClient _smsClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AcsSmsSender> _logger;

        public AcsSmsSender(
            SmsClient smsClient,
            IConfiguration configuration,
            ILogger<AcsSmsSender> logger)
        {
            _smsClient = smsClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendAsync(string toPhoneNumber, string message, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(toPhoneNumber))
            {
                _logger.LogWarning("SMS recipient is empty.");
                return;
            }

            var fromPhoneNumber = _configuration["CommunicationServices:Sms:FromPhoneNumber"];
            if (string.IsNullOrWhiteSpace(fromPhoneNumber))
            {
                _logger.LogWarning("CommunicationServices:Sms:FromPhoneNumber is not configured.");
                return;
            }

            var response = await _smsClient.SendAsync(
                fromPhoneNumber,
                toPhoneNumber,
                message,
                new SmsSendOptions(enableDeliveryReport: true),
                ct);

            if (!response.Value.Successful)
            {
                _logger.LogWarning("SMS send failed: {ErrorMessage}", response.Value.ErrorMessage);
            }
        }
    }
}
