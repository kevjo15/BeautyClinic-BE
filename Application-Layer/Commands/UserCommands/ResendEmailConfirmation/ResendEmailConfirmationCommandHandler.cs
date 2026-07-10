using Application_Layer.Interfaces;
using Domain_Layer.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application_Layer.Commands.UserCommands.ResendEmailConfirmation
{
    public class ResendEmailConfirmationCommandHandler
        : IRequestHandler<ResendEmailConfirmationCommand, OperationResult>
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailConfirmationEmailService _confirmationEmailService;
        private readonly ILogger<ResendEmailConfirmationCommandHandler> _logger;

        public ResendEmailConfirmationCommandHandler(
            IUserRepository userRepository,
            IEmailConfirmationEmailService confirmationEmailService,
            ILogger<ResendEmailConfirmationCommandHandler> logger)
        {
            _userRepository = userRepository;
            _confirmationEmailService = confirmationEmailService;
            _logger = logger;
        }

        public async Task<OperationResult> Handle(ResendEmailConfirmationCommand request, CancellationToken cancellationToken)
        {
            var email = request.Request.Email.Trim();

            // Svara alltid Success — avslöja inte om adressen finns eller redan är bekräftad.
            var tokenInfo = await _userRepository.GenerateEmailConfirmationTokenAsync(email);
            if (tokenInfo == null)
            {
                _logger.LogInformation("Email confirmation resend requested for unknown or already confirmed address");
                return OperationResult.Success();
            }

            var user = await _userRepository.FindByEmailAsync(email);

            try
            {
                await _confirmationEmailService.SendConfirmationLinkAsync(
                    email, user?.FirstName, tokenInfo.Value.UserId, tokenInfo.Value.Token, cancellationToken);
                _logger.LogInformation("Email confirmation resent for user {UserId}", tokenInfo.Value.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to resend confirmation email for user {UserId}", tokenInfo.Value.UserId);
            }

            return OperationResult.Success();
        }
    }
}
