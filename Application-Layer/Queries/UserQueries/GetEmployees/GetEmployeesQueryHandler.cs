using Application_Layer.DTOs;
using Application_Layer.Interfaces;
using Application_Layer.Mapping;
using MediatR;

namespace Application_Layer.Queries.UserQueries.GetEmployees
{
    public class GetEmployeesQueryHandler : IRequestHandler<GetEmployeesQuery, List<EmployeeDTO>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IApplicationMapper _mapper;

        public GetEmployeesQueryHandler(IUserRepository userRepository, IApplicationMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<List<EmployeeDTO>> Handle(GetEmployeesQuery request, CancellationToken cancellationToken)
        {
            var employees = await _userRepository.GetEmployeesAsync();
            return _mapper.ToEmployeeDtoList(employees);
        }
    }
}
