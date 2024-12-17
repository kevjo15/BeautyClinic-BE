using Application_Layer.Jwt;
using AutoMapper;
using Domain_Layer.Models;
using Infrastructure_Layer.Repositories.User;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Application_Layer.Commands.UserCommands.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResult>
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IMapper _mapper;

        public LoginCommandHandler(IUserRepository userRepository, IJwtTokenGenerator jwtTokenGenerator, IMapper mapper)
        {
            _userRepository = userRepository;
            _jwtTokenGenerator = jwtTokenGenerator;
            _mapper = mapper;
        }

        public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var existingUser = await _userRepository.FindByEmailAsync(request.LoginUserDTO.Email);
                if (existingUser == null)
                {
                    return CreateLoginResult(false, "Användaren existerar inte.");
                }

                var passwordValid = await _userRepository.CheckPasswordAsync(existingUser, request.LoginUserDTO.Password);
                if (!passwordValid)
                {
                    return CreateLoginResult(false, "Felaktigt lösenord.");
                }

                var token = await _jwtTokenGenerator.GenerateToken(existingUser);

                // Generate and store refresh token
                var refreshToken = GenerateRefreshToken();
                existingUser.RefreshToken = refreshToken;
                existingUser.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7); // Example expiry time
                await _userRepository.UpdateUserAsync(existingUser);

                return CreateLoginResult(true, null, token, refreshToken);
            }
            catch (Exception ex)
            {
                return CreateLoginResult(false, $"An unexpected error occurred: {ex.Message}");
            }
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        private LoginResult CreateLoginResult(bool successful, string? error, string? token = null, string? refreshToken = null)
        {
            return new LoginResult { Successful = successful, Error = error, Token = token, RefreshToken = refreshToken };
        }
    }
}
