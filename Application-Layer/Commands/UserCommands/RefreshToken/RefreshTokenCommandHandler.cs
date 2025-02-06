using Application_Layer.Interfaces;
using Application_Layer.Jwt;
using MediatR;

namespace Application_Layer.Commands.UserCommands.RefreshToken
{
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, RefreshTokenResult>
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public RefreshTokenCommandHandler(IUserRepository userRepository, IJwtTokenGenerator jwtTokenGenerator)
        {
            _userRepository = userRepository;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<RefreshTokenResult> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.FindByRefreshTokenAsync(request.RefreshToken);
            if (user == null)
            {
                return new RefreshTokenResult(false, "Invalid refresh token.");
            }

            var newAccessToken = await _jwtTokenGenerator.GenerateToken(user);
            return new RefreshTokenResult(true, null, newAccessToken);
        }
    }
}
