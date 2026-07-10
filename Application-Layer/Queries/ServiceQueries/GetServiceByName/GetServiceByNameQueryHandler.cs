using MediatR;
using Application_Layer.DTOs;
using Application_Layer.Mapping;
using Application_Layer.Interfaces;

namespace Application_Layer.Queries.ServiceQueries
{
    public class GetServiceByNameQueryHandler : IRequestHandler<GetServiceByNameQuery, IEnumerable<ServiceDTO>>
    {
        private readonly IServiceRepository _serviceRepository;
        private readonly IServiceImageUrlResolver _imageUrlResolver;
        private readonly IApplicationMapper _mapper;

        public GetServiceByNameQueryHandler(
            IServiceRepository serviceRepository,
            IServiceImageUrlResolver imageUrlResolver,
            IApplicationMapper mapper)
        {
            _serviceRepository = serviceRepository;
            _imageUrlResolver = imageUrlResolver;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ServiceDTO>> Handle(GetServiceByNameQuery request, CancellationToken cancellationToken)
        {
            var services = await _serviceRepository.GetAllServicesAsync();
            var filteredServices = services.Where(s => s.Name.Contains(request.Name, StringComparison.OrdinalIgnoreCase));
            var dtos = _mapper.ToServiceDtoList(filteredServices);
            await _imageUrlResolver.ApplyAsync(dtos, cancellationToken);
            return dtos;
        }
    }
}
