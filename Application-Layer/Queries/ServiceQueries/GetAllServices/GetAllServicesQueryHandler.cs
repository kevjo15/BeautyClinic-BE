using Application_Layer.DTOs;
using Application_Layer.Interfaces;
using Application_Layer.Mapping;
using MediatR;

namespace Application_Layer.Queries.ServiceQueries.GetAllServices
{
    public class GetAllServicesQueryHandler : IRequestHandler<GetAllServicesQuery, IEnumerable<ServiceDTO>>
    {
        private readonly IServiceRepository _serviceRepository;
        private readonly IServiceImageUrlResolver _imageUrlResolver;
        private readonly IApplicationMapper _mapper;

        public GetAllServicesQueryHandler(
            IServiceRepository serviceRepository,
            IServiceImageUrlResolver imageUrlResolver,
            IApplicationMapper mapper)
        {
            _serviceRepository = serviceRepository;
            _imageUrlResolver = imageUrlResolver;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ServiceDTO>> Handle(GetAllServicesQuery request, CancellationToken cancellationToken)
        {
            var services = await _serviceRepository.GetAllServicesAsync();
            var dtos = _mapper.ToServiceDtoList(services);
            await _imageUrlResolver.ApplyAsync(dtos, cancellationToken);
            return dtos;
        }
    }
}
