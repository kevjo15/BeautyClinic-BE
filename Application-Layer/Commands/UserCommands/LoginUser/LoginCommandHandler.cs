using Application_Layer.Jwt;
using AutoMapper;
using Domain_Layer.Models;
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
                var user = _mapper.Map<UserModel>(request.LoginUserDTO);

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
                return CreateLoginResult(true, null, token);
            }
            catch (Exception ex)
            {
                return CreateLoginResult(false, $"An unexpected error occurred: {ex.Message}");
            }

        }
        private LoginResult CreateLoginResult(bool successful, string? error, string? token = null)
        {
            return new LoginResult { Successful = successful, Error = error, Token = token };
        }
    }
}
