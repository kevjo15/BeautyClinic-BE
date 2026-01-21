using System.Net;
using Application_Layer.DTOs;
using Application_Layer.Interfaces;
using API_Layer.Hubs;
using Domain_Layer.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace API_Layer.Services
{
    public class SignalRNotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub, INotificationClient> _hubContext;
        private readonly IUserRepository _userRepository;
        private readonly IEmailSender _emailSender;
        private readonly ISmsSender _smsSender;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SignalRNotificationService> _logger;

        public SignalRNotificationService(
            IHubContext<NotificationHub, INotificationClient> hubContext,
            IUserRepository userRepository,
            IEmailSender emailSender,
            ISmsSender smsSender,
            IConfiguration configuration,
            ILogger<SignalRNotificationService> logger)
        {
            _hubContext = hubContext;
            _userRepository = userRepository;
            _emailSender = emailSender;
            _smsSender = smsSender;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendBookingNotificationAsync(
            string userId,
            string title,
            string message,
            NotificationType type,
            Guid bookingId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("Notification userId is empty.");
                return;
            }

            var notificationDto = new NotificationDTO
            {
                Id = Guid.NewGuid(),
                Title = title,
                Message = message,
                Type = type,
                CreatedAt = DateTime.UtcNow,
                IsRead = false,
                UserId = userId
            };

            try
            {
                await _hubContext.Clients.Group(userId).ReceiveNotification(notificationDto);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "SignalR notification failed for user {UserId}", userId);
            }

            var enableEmail = _configuration.GetValue<bool>("Notifications:EnableEmail");
            var enableSms = _configuration.GetValue<bool>("Notifications:EnableSms");

            if (!enableEmail && !enableSms)
            {
                return;
            }

            UserModel? user = null;
            try
            {
                user = await _userRepository.FindByIdAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load user for notifications {UserId}", userId);
            }

            if (enableEmail && !string.IsNullOrWhiteSpace(user?.Email))
            {
                var brandName = _configuration["Notifications:BrandName"];
                var logoUrl = _configuration["Notifications:BrandLogoUrl"];
                var supportEmail = _configuration["Notifications:SupportEmail"];
                var appBaseUrl = _configuration["Notifications:AppBaseUrl"];

                var template = EmailTemplateBuilder.BuildBookingTemplate(
                    title,
                    message,
                    brandName,
                    logoUrl,
                    supportEmail,
                    appBaseUrl);

                try
                {
                    await _emailSender.SendAsync(user.Email, title, template.PlainText, template.Html, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Email notification failed for user {UserId}", userId);
                }
            }

            if (enableSms && !string.IsNullOrWhiteSpace(user?.PhoneNumber))
            {
                var smsMessage = $"{title}: {message}";
                try
                {
                    await _smsSender.SendAsync(user.PhoneNumber, smsMessage, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "SMS notification failed for user {UserId}", userId);
                }
            }
        }
    }
}
