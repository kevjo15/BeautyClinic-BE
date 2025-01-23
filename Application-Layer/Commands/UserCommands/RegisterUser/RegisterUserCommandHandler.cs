using Application_Layer.Interfaces;
using AutoMapper;
using Domain_Layer.Models;
using MediatR;

namespace Application_Layer.Commands.UserCommands.RegisterUser
{
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, RegisterResult>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public RegisterUserCommandHandler(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<RegisterResult> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {

            try
            {
                var user = _mapper.Map<UserModel>(request.NewUser);

                var result = await _userRepository.RegisterUserAsync(user, request.NewUser.Password);

                if (!result.Succeeded)
                {
                    return new RegisterResult(false, null, result.Errors.Select(e => e.Description).ToList());
                }

                return new RegisterResult(true, user);
            }
            catch (Exception ex)
            {
                return new RegisterResult(false, null, new List<string> { "An unexpected error occurred: " + ex.Message });
            }
        }
    }
}
