using Application_Layer.Interfaces;
using AutoMapper;
using Domain_Layer.Common;
using Domain_Layer.Models;
using MediatR;

namespace Application_Layer.Commands.UserCommands.RegisterUser
{
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, OperationResult<UserModel>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public RegisterUserCommandHandler(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<OperationResult<UserModel>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {

            try
            {
                var user = _mapper.Map<UserModel>(request.NewUser);
                user.UserName = request.NewUser.Email;

                var result = await _userRepository.RegisterUserAsync(user, request.NewUser.Password);

                if (!result.Successful)
                {
                    return OperationResult<UserModel>.Failure(result.Error ?? "Failed to register user.");
                }

                return OperationResult<UserModel>.Success(user);
            }
            catch (Exception ex)
            {
                return OperationResult<UserModel>.Failure("An unexpected error occurred: " + ex.Message);
            }
        }
    }
}
