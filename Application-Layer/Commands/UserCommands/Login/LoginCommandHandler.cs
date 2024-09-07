using Application_Layer.Jwt;
using Infrastructure_Layer.Repositories.User;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application_Layer.Commands.UserCommands.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResult>
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public LoginCommandHandler(IUserRepository userRepository, IJwtTokenGenerator jwtTokenGenerator)
        {
            _userRepository = userRepository;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.FindByEmailAsync(request.LoginUserDTO.Email);
            if (user == null)
            {
                return CreateLoginResult(false, "Användaren existerar inte.");
            }

            var passwordValid = await _userRepository.CheckPasswordAsync(user, request.LoginUserDTO.Password);
            if(!passwordValid)
            {
                return CreateLoginResult(false, "Felaktigt lösenord.");
            }

            var token = _jwtTokenGenerator.GenerateToken(user);
            return CreateLoginResult(true, null, token);

        }
        private LoginResult CreateLoginResult(bool successful, string? error, string? token = null)
        {
            return new LoginResult { Successful = successful, Error = error, Token = token };
        }
    }
}
