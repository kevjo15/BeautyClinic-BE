using Application_Layer.DTOs;
using Application_Layer.Interfaces;
using Application_Layer.Mapping;
using MediatR;

namespace Application_Layer.Queries.UserQueries.GetMyProfile
{
    public class GetMyProfileQueryHandler : IRequestHandler<GetMyProfileQuery, UserProfileDTO?>
    {
        private readonly IUserRepository _userRepository;
        private readonly IServiceImageUrlResolver _imageUrlResolver;
        private readonly IApplicationMapper _mapper;

        public GetMyProfileQueryHandler(
            IUserRepository userRepository,
            IServiceImageUrlResolver imageUrlResolver,
            IApplicationMapper mapper)
        {
            _userRepository = userRepository;
            _imageUrlResolver = imageUrlResolver;
            _mapper = mapper;
        }

        public async Task<UserProfileDTO?> Handle(GetMyProfileQuery request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.FindByIdAsync(request.UserId);
            if (user == null)
            {
                return null;
            }

            var dto = _mapper.ToUserProfileDto(user);
            dto.AvatarUrl = string.IsNullOrWhiteSpace(user.AvatarUrl)
                ? null
                : await _imageUrlResolver.ResolveAsync(user.AvatarUrl, cancellationToken);
            return dto;
        }
    }
}
