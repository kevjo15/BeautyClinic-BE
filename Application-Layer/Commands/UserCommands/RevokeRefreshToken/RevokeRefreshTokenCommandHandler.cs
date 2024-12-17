using Infrastructure_Layer.Repositories.User;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application_Layer.Commands.UserCommands.RevokeRefreshToken
{
    public class RevokeRefreshTokenCommandHandler : IRequestHandler<RevokeRefreshTokenCommand, bool>
    {
        private readonly IUserRepository _userRepository;

        public RevokeRefreshTokenCommandHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<bool> Handle(RevokeRefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.FindByIdAsync(request.UserId);
            if (user == null) return false;

            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            await _userRepository.UpdateUserAsync(user);

            return true;
        }
    }
} 