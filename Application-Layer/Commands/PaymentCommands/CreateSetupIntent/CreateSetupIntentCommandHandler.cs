using Application_Layer.DTOs;
using Application_Layer.Interfaces;
using Domain_Layer.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application_Layer.Commands.PaymentCommands.CreateSetupIntent
{
    public class CreateSetupIntentCommandHandler
        : IRequestHandler<CreateSetupIntentCommand, OperationResult<SetupIntentResponseDTO>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IStripePaymentService _stripe;
        private readonly ILogger<CreateSetupIntentCommandHandler> _logger;

        public CreateSetupIntentCommandHandler(
            IUserRepository userRepository,
            IStripePaymentService stripe,
            ILogger<CreateSetupIntentCommandHandler> logger)
        {
            _userRepository = userRepository;
            _stripe = stripe;
            _logger = logger;
        }

        public async Task<OperationResult<SetupIntentResponseDTO>> Handle(
            CreateSetupIntentCommand request, CancellationToken cancellationToken)
        {
            if (!_stripe.IsConfigured)
            {
                return OperationResult<SetupIntentResponseDTO>.Failure("Betalning är inte konfigurerad.");
            }

            var user = await _userRepository.FindByIdAsync(request.UserId);
            if (user == null)
            {
                return OperationResult<SetupIntentResponseDTO>.Failure("User not found.", OperationFailureType.NotFound);
            }

            try
            {
                var fullName = $"{user.FirstName} {user.LastName}".Trim();
                var customerId = await _stripe.EnsureCustomerAsync(
                    user.Id, user.Email, string.IsNullOrWhiteSpace(fullName) ? null : fullName,
                    user.StripeCustomerId, cancellationToken);

                // Persistera customer-id:t om det just skapades.
                if (customerId != user.StripeCustomerId)
                {
                    await _userRepository.SetStripeCustomerIdAsync(user.Id, customerId);
                }

                var clientSecret = await _stripe.CreateSetupIntentAsync(customerId, cancellationToken);
                return OperationResult<SetupIntentResponseDTO>.Success(
                    new SetupIntentResponseDTO { ClientSecret = clientSecret });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create Stripe SetupIntent for user {UserId}", request.UserId);
                return OperationResult<SetupIntentResponseDTO>.Failure("Kunde inte förbereda kortbetalning. Försök igen.");
            }
        }
    }
}
