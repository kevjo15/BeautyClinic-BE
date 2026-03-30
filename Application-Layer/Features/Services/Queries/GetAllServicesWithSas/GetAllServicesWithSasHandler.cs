using Application_Layer.DTOs;
using Application_Layer.Interfaces;
using AutoMapper;
using MediatR;
using System.Collections.Generic;
using System.Linq;

namespace Application_Layer.Features.Services.Queries.GetAllServicesWithSas
{
    public sealed class GetAllServicesWithSasHandler
        : IRequestHandler<GetAllServicesWithSasQuery, IReadOnlyList<ServiceDTO>>
    {
        private readonly IServiceRepository _repo;
        private readonly IFileService _files;
        private readonly IMapper _mapper;

        public GetAllServicesWithSasHandler(
            IServiceRepository repo,
            IFileService files,
            IMapper mapper)
        {
            _repo = repo;
            _files = files;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<ServiceDTO>> Handle(
            GetAllServicesWithSasQuery request,
            CancellationToken ct)
        {
            var services = (await _repo.GetAllServicesAsync()).ToList();
            var serviceDtos = _mapper.Map<List<ServiceDTO>>(services);

            for (int i = 0; i < serviceDtos.Count; i++)
            {
                var service = services[i];
                if (!string.IsNullOrWhiteSpace(service.ImageUrl))
                {
                    serviceDtos[i].ImageUrl = await _files.GenerateServiceImageReadUrlAsync(service.ImageUrl, ct);
                }
            }

            return serviceDtos;
        }
    }
}
