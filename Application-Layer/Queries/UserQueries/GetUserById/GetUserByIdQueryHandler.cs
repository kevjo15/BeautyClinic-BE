using Application_Layer.DTOs;
using Application_Layer.Interfaces;
using Application_Layer.Mapping;
using MediatR;

namespace Application_Layer.Queries.UserQueries.GetUserById
{
    public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, GetUserByIdResponseDTO>
    {
        private readonly IUserRepository _userRepository;
        private readonly IApplicationMapper _mapper;

        public GetUserByIdQueryHandler(IUserRepository userRepository, IApplicationMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }
        public async Task<GetUserByIdResponseDTO> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.FindByIdAsync(request.UserId);

            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {request.UserId} not found.");
            }

            return _mapper.ToUserByIdResponseDto(user);
        }
    }
}
