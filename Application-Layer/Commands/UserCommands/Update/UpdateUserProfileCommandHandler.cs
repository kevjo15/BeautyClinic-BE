using Application_Layer.DTO_s;
using Application_Layer.Interfaces;
using AutoMapper;
using Domain_Layer.Common;
using MediatR;

namespace Application_Layer.Commands.UserCommands.Update
{
    public class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, OperationResult<UpdateUserProfileDTO>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        public UpdateUserProfileCommandHandler(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<OperationResult<UpdateUserProfileDTO>> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.FindByIdAsync(request.UserId);

            if (user == null)
            {
                return OperationResult<UpdateUserProfileDTO>.Failure("User was not found!");
            }

            _mapper.Map(request.UpdatedProfileDTO, user);

            var updateResult = await _userRepository.UpdateUserAsync(user);

            if (!updateResult.Successful)
            {
                return OperationResult<UpdateUserProfileDTO>.Failure(updateResult.Error ?? "Failed to update user profile.");
            }
            var updatedProfile = _mapper.Map<UpdateUserProfileDTO>(user);
            return OperationResult<UpdateUserProfileDTO>.Success(updatedProfile);
        }
    }
}
