using Application_Layer.DTO_s;
using Application_Layer.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application_Layer.Features.Services.Queries.GetAllServicesWithSas
{
    public sealed class GetAllServicesWithSasHandler
        : IRequestHandler<GetAllServicesWithSasQuery, IReadOnlyList<ServiceDTO>>
    {
        private readonly IServiceRepository _repo;
        private readonly IFileService _files;
        private readonly IConfiguration _cfg;
        private readonly IMapper _mapper;

        public GetAllServicesWithSasHandler(
            IServiceRepository repo,
            IFileService files,
            IConfiguration cfg,
            IMapper mapper)
        {
            _repo = repo;
            _files = files;
            _cfg = cfg;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<ServiceDTO>> Handle(
            GetAllServicesWithSasQuery request,
            CancellationToken ct)
        {
            var services = (await _repo.GetAllServicesAsync()).ToList();
            var serviceDtos = _mapper.Map<List<ServiceDTO>>(services);

            var lifeHours = int.TryParse(_cfg["Storage:SasHours"], out var h) ? h : 12;
            var ttl = TimeSpan.FromHours(lifeHours);
            var containerName = _cfg["Storage:Containers:Images"] ?? "images";

            for (int i = 0; i < serviceDtos.Count; i++)
            {
                var service = services[i];
                if (!string.IsNullOrWhiteSpace(service.ImageUrl))
                {
                    serviceDtos[i].ImageUrl =
                        await _files.GenerateReadSasAsync(containerName, service.ImageUrl, ttl, ct);
                }
            }

            return serviceDtos;
        }
    }
}
