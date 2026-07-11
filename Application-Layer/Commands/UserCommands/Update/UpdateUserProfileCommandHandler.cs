using Application_Layer.DTOs;
using Application_Layer.Interfaces;
using Application_Layer.Mapping;
using Domain_Layer.Common;
using MediatR;

namespace Application_Layer.Commands.UserCommands.Update
{
    public class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, OperationResult<UpdateUserProfileDTO>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IApplicationMapper _mapper;
        public UpdateUserProfileCommandHandler(IUserRepository userRepository, IApplicationMapper mapper)
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

            _mapper.UpdateUserModel(request.UpdatedProfileDTO, user);

            var updateResult = await _userRepository.UpdateUserAsync(user);

            if (!updateResult.Successful)
            {
                return OperationResult<UpdateUserProfileDTO>.Failure(updateResult.Error ?? "Failed to update user profile.");
            }
            var updatedProfile = _mapper.ToUpdateUserProfileDto(user);
            return OperationResult<UpdateUserProfileDTO>.Success(updatedProfile);
        }
    }
}
