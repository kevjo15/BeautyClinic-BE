using Application_Layer.DTO_s;
using Application_Layer.Interfaces;
using AutoMapper;
using MediatR;

namespace Application_Layer.Commands.UserCommands.Update
{
    public class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, UpdateUserProfileResult>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        public UpdateUserProfileCommandHandler(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<UpdateUserProfileResult> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.FindByIdAsync(request.UserId);

            if (user == null)
            {
                return new UpdateUserProfileResult(false, null, new List<string> { "User was not found!" });
            }

            _mapper.Map(request.UpdatedProfileDTO, user);

            var updateResult = await _userRepository.UpdateUserAsync(user);

            if (!updateResult.Succeeded)
            {
                return new UpdateUserProfileResult(false, null, updateResult.Errors.Select(e => e.Description).ToList());
            }
            var updatedProfile = _mapper.Map<UpdateUserProfileDTO>(user);
            return new UpdateUserProfileResult(true, updatedProfile);
        }
    }
}
