using MediatR;
using Application_Layer.DTOs;
using Application_Layer.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace Application_Layer.Queries.UserQueries.GetUserName
{
    public class GetUserNameQueryHandler : IRequestHandler<GetUserNameQuery, UserNameDTO>
    {
        private readonly IUserRepository _userRepository;

        public GetUserNameQueryHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UserNameDTO> Handle(GetUserNameQuery request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.FindByIdAsync(request.UserId);

            if (user == null)
            {
                return null;
            }

            return new UserNameDTO
            {
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty
            };
        }
    }
}
