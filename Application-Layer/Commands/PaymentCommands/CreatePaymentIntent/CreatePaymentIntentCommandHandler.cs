using Application_Layer.Common;
using Application_Layer.DTOs;
using Application_Layer.Interfaces;
using Domain_Layer.Common;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Application_Layer.Commands.PaymentCommands.CreatePaymentIntent
{
    public class CreatePaymentIntentCommandHandler
        : IRequestHandler<CreatePaymentIntentCommand, OperationResult<PaymentIntentResponseDTO>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IServiceRepository _serviceRepository;
        private readonly IStripePaymentService _stripe;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CreatePaymentIntentCommandHandler> _logger;

        public CreatePaymentIntentCommandHandler(
            IUserRepository userRepository,
            IServiceRepository serviceRepository,
            IStripePaymentService stripe,
            IConfiguration configuration,
            ILogger<CreatePaymentIntentCommandHandler> logger)
        {
            _userRepository = userRepository;
            _serviceRepository = serviceRepository;
            _stripe = stripe;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<OperationResult<PaymentIntentResponseDTO>> Handle(
            CreatePaymentIntentCommand request, CancellationToken cancellationToken)
        {
            if (!_stripe.IsConfigured)
            {
                return OperationResult<PaymentIntentResponseDTO>.Failure("Betalning är inte konfigurerad.");
            }

            var user = await _userRepository.FindByIdAsync(request.UserId);
            if (user == null)
            {
                return OperationResult<PaymentIntentResponseDTO>.Failure("User not found.", OperationFailureType.NotFound);
            }

            var service = await _serviceRepository.GetServiceByIdAsync(request.ServiceId);
            if (service == null)
            {
                return OperationResult<PaymentIntentResponseDTO>.Failure("Behandlingen hittades inte.", OperationFailureType.NotFound);
            }

            try
            {
                var fullName = $"{user.FirstName} {user.LastName}".Trim();
                var customerId = await _stripe.EnsureCustomerAsync(
                    user.Id, user.Email, string.IsNullOrWhiteSpace(fullName) ? null : fullName,
                    user.StripeCustomerId, cancellationToken);
                if (customerId != user.StripeCustomerId)
                {
                    await _userRepository.SetStripeCustomerIdAsync(user.Id, customerId);
                }

                var currency = _configuration["Stripe:Currency"] ?? "sek";
                var amountKr = service.Price;

                var intent = await _stripe.CreatePaymentIntentAsync(
                    customerId, PaymentAmounts.ToMinorUnit(amountKr), currency,
                    $"Betalning för {service.Name}",
                    // Bokningsuppgifter i metadatan → betalningen kan stämmas av till en
                    // bokning via /payment-return ELLER webhook (orphan-skydd).
                    new Dictionary<string, string>
                    {
                        ["userId"] = user.Id,
                        ["serviceId"] = service.Id.ToString(),
                        ["employeeId"] = request.EmployeeId ?? string.Empty,
                        ["startTime"] = request.StartTime.ToString("o"),
                        ["endTime"] = request.EndTime.ToString("o"),
                    },
                    cancellationToken);

                return OperationResult<PaymentIntentResponseDTO>.Success(new PaymentIntentResponseDTO
                {
                    ClientSecret = intent.ClientSecret,
                    Amount = amountKr,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create Stripe PaymentIntent for user {UserId}", request.UserId);
                return OperationResult<PaymentIntentResponseDTO>.Failure("Kunde inte förbereda betalningen. Försök igen.");
            }
        }
    }
}
