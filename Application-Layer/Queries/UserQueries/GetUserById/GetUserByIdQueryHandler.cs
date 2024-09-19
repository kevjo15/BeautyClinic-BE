using Application_Layer.DTO_s;
using AutoMapper;
using Domain_Layer.Models;
using Infrastructure_Layer.Repositories.User;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application_Layer.Queries.UserQueries.GetUserById
{
    public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, GetUserByIdResponseDTO>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public GetUserByIdQueryHandler(IUserRepository userRepository, IMapper mapper)
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

            return _mapper.Map<GetUserByIdResponseDTO>(user);
        }
    }
}
