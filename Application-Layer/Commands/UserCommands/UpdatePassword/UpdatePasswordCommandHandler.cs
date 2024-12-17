using Infrastructure_Layer.Repositories.User;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application_Layer.Commands.UserCommands.UpdatePassword
{
    public class UpdatePasswordCommandHandler : IRequestHandler<UpdatePasswordCommand, bool>
    {
        private readonly IUserRepository _userRepository;

        public UpdatePasswordCommandHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<bool> Handle(UpdatePasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.FindByIdAsync(request.UserId);
            if (user == null) return false;

            var passwordValid = await _userRepository.CheckPasswordAsync(user, request.UpdatePasswordDTO.CurrentPassword);
            if (!passwordValid) return false;

            if (request.UpdatePasswordDTO.CurrentPassword == request.UpdatePasswordDTO.NewPassword)
            {
                return false; // New password must be different
            }

            var result = await _userRepository.UpdatePasswordAsync(user, request.UpdatePasswordDTO.NewPassword);
            return result.Succeeded;
        }
    }
} 