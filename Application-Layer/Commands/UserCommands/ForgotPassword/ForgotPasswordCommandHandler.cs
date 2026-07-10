using Application_Layer.Interfaces;
using Domain_Layer.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application_Layer.Commands.UserCommands.ForgotPassword
{
    public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, OperationResult>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordResetEmailService _resetEmailService;
        private readonly ILogger<ForgotPasswordCommandHandler> _logger;

        public ForgotPasswordCommandHandler(
            IUserRepository userRepository,
            IPasswordResetEmailService resetEmailService,
            ILogger<ForgotPasswordCommandHandler> logger)
        {
            _userRepository = userRepository;
            _resetEmailService = resetEmailService;
            _logger = logger;
        }

        public async Task<OperationResult> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            var email = request.Request.Email.Trim();

            // Svara alltid Success — endpointen får inte avslöja vilka adresser som finns.
            var token = await _userRepository.GeneratePasswordResetTokenAsync(email);
            if (token == null)
            {
                _logger.LogInformation("Password reset requested for unknown email");
                return OperationResult.Success();
            }

            var user = await _userRepository.FindByEmailAsync(email);

            try
            {
                await _resetEmailService.SendResetLinkAsync(email, user?.FirstName, token, cancellationToken);
                _logger.LogInformation("Password reset email dispatched for user {UserId}", user?.Id);
            }
            catch (Exception ex)
            {
                // Mejlfel får inte läcka ut till anroparen; token blir ändå ogiltig vid nästa begäran.
                _logger.LogError(ex, "Failed to send password reset email for user {UserId}", user?.Id);
            }

            return OperationResult.Success();
        }
    }
}
