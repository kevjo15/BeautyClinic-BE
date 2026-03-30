using Application_Layer.Interfaces;
using Domain_Layer.Common;
using MediatR;

namespace Application_Layer.Commands.UserCommands.UpdatePassword
{
    public class UpdatePasswordCommandHandler : IRequestHandler<UpdatePasswordCommand, OperationResult>
    {
        private readonly IUserRepository _userRepository;

        public UpdatePasswordCommandHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<OperationResult> Handle(UpdatePasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.FindByIdAsync(request.UserId);
            if (user == null) return OperationResult.Failure("User not found.");

            var passwordValid = await _userRepository.CheckPasswordAsync(user, request.UpdatePasswordDTO.CurrentPassword);
            if (!passwordValid) return OperationResult.Failure("Current password is incorrect.");

            if (request.UpdatePasswordDTO.CurrentPassword == request.UpdatePasswordDTO.NewPassword)
            {
                return OperationResult.Failure("New password must be different from the current password.");
            }

            var result = await _userRepository.UpdatePasswordAsync(user, request.UpdatePasswordDTO.NewPassword);
            return result;
        }
    }
}
