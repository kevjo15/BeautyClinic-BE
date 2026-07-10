using MediatR;
using Application_Layer.DTOs;
using Application_Layer.Mapping;
using Application_Layer.Interfaces;

public class GetServicesByCategoryQueryHandler : IRequestHandler<GetServicesByCategoryQuery, IEnumerable<ServiceDTO>>
{
    private readonly IServiceRepository _serviceRepository;
    private readonly IServiceImageUrlResolver _imageUrlResolver;
    private readonly IApplicationMapper _mapper;

    public GetServicesByCategoryQueryHandler(
        IServiceRepository serviceRepository,
        IServiceImageUrlResolver imageUrlResolver,
        IApplicationMapper mapper)
    {
        _serviceRepository = serviceRepository;
        _imageUrlResolver = imageUrlResolver;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ServiceDTO>> Handle(GetServicesByCategoryQuery request, CancellationToken cancellationToken)
    {
        var services = await _serviceRepository.GetServicesByCategoryAsync(request.CategoryId);
        var dtos = _mapper.ToServiceDtoList(services);
        await _imageUrlResolver.ApplyAsync(dtos, cancellationToken);
        return dtos;
    }
}
