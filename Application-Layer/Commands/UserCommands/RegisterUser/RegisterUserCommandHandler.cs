using Application_Layer.Interfaces;
using Application_Layer.Mapping;
using Domain_Layer.Common;
using Domain_Layer.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application_Layer.Commands.UserCommands.RegisterUser
{
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, OperationResult<UserModel>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailConfirmationEmailService _confirmationEmailService;
        private readonly ILogger<RegisterUserCommandHandler> _logger;
        private readonly IApplicationMapper _mapper;

        public RegisterUserCommandHandler(
            IUserRepository userRepository,
            IEmailConfirmationEmailService confirmationEmailService,
            ILogger<RegisterUserCommandHandler> logger,
            IApplicationMapper mapper)
        {
            _userRepository = userRepository;
            _confirmationEmailService = confirmationEmailService;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<OperationResult<UserModel>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {

            try
            {
                var user = _mapper.ToUserModel(request.NewUser);
                user.UserName = request.NewUser.Email;

                var result = await _userRepository.RegisterUserAsync(user, request.NewUser.Password);

                if (!result.Successful)
                {
                    return OperationResult<UserModel>.Failure(result.Error ?? "Failed to register user.");
                }

                await SendConfirmationEmailAsync(user, cancellationToken);

                return OperationResult<UserModel>.Success(user);
            }
            catch (Exception ex)
            {
                return OperationResult<UserModel>.Failure("An unexpected error occurred: " + ex.Message);
            }
        }

        private async Task SendConfirmationEmailAsync(UserModel user, CancellationToken ct)
        {
            // Mejlfel får inte fälla registreringen — användaren kan begära nytt mejl.
            try
            {
                var tokenInfo = await _userRepository.GenerateEmailConfirmationTokenAsync(user.Email!);
                if (tokenInfo != null)
                {
                    await _confirmationEmailService.SendConfirmationLinkAsync(
                        user.Email!, user.FirstName, tokenInfo.Value.UserId, tokenInfo.Value.Token, ct);
                    _logger.LogInformation("Confirmation email dispatched for user {UserId}", tokenInfo.Value.UserId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send confirmation email for new user {UserId}", user.Id);
            }
        }
    }
}
