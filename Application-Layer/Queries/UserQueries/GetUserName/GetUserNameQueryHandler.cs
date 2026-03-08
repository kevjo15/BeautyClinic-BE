using AutoMapper;
using MediatR;
using Application_Layer.DTOs;
using Application_Layer.Interfaces;

namespace Application_Layer.Queries.UserQueries.GetUserName
{
    public class GetUserNameQueryHandler : IRequestHandler<GetUserNameQuery, UserNameDTO>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public GetUserNameQueryHandler(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<UserNameDTO> Handle(GetUserNameQuery request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.FindByIdAsync(request.UserId);

            if (user == null)
                return null;

            return _mapper.Map<UserNameDTO>(user);
        }
    }
}
