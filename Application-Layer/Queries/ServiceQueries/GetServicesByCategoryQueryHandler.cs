using MediatR;
using Infrastructure_Layer.Repositories.Service;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application_Layer.DTO_s;
using AutoMapper;

public class GetServicesByCategoryQueryHandler : IRequestHandler<GetServicesByCategoryQuery, IEnumerable<ServiceDTO>>
{
    private readonly IServiceRepository _serviceRepository;
    private readonly IMapper _mapper;

    public GetServicesByCategoryQueryHandler(IServiceRepository serviceRepository, IMapper mapper)
    {
        _serviceRepository = serviceRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ServiceDTO>> Handle(GetServicesByCategoryQuery request, CancellationToken cancellationToken)
    {
        var services = await _serviceRepository.GetServicesByCategoryAsync(request.CategoryId);
        return _mapper.Map<IEnumerable<ServiceDTO>>(services);
    }
} 