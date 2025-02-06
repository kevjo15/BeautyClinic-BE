using MediatR;
using Application_Layer.DTO_s;
using AutoMapper;
using Application_Layer.Interfaces;

namespace Application_Layer.Queries.ServiceQueries
{
    public class GetServiceByNameQueryHandler : IRequestHandler<GetServiceByNameQuery, IEnumerable<ServiceDTO>>
    {
        private readonly IServiceRepository _serviceRepository;
        private readonly IMapper _mapper;

        public GetServiceByNameQueryHandler(IServiceRepository serviceRepository, IMapper mapper)
        {
            _serviceRepository = serviceRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ServiceDTO>> Handle(GetServiceByNameQuery request, CancellationToken cancellationToken)
        {
            var services = await _serviceRepository.GetAllServicesAsync();
            var filteredServices = services.Where(s => s.Name.Contains(request.Name, StringComparison.OrdinalIgnoreCase));
            return _mapper.Map<IEnumerable<ServiceDTO>>(filteredServices);
        }
    }
}