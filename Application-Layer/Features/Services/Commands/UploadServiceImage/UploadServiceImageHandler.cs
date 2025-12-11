using Application_Layer.Interfaces;
using Application_Layer.Services;
using MediatR;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application_Layer.Features.Services.Commands.UploadServiceImage
{
    public sealed class UploadServiceImageHandler : IRequestHandler<UploadServiceImageCommand, UploadServiceImageResult>
    {
        private readonly IServiceRepository _repo;
        private readonly IFileService _files;
        private readonly IConfiguration _cfg;

        public UploadServiceImageHandler(IServiceRepository repo, IFileService files, IConfiguration cfg)
        {
            _repo = repo;
            _files = files;
            _cfg = cfg;
        }

        public async Task<UploadServiceImageResult> Handle(UploadServiceImageCommand request, CancellationToken ct)
        {
            var service = await _repo.GetServiceByIdAsync(request.ServiceId)
                ?? throw new KeyNotFoundException("Service not found");

            var container = _cfg["Storage:Containers:Services"] ?? "services";
            var lifeHours = int.TryParse(_cfg["Storage:SasHours"], out var h) ? h : 12;
            var ttl = TimeSpan.FromHours(lifeHours);

            var (blobPath, sas) = await _files.UploadAsync(request.File, container, ttl, ct);

            service.ImageUrl = blobPath;
            await _repo.UpdateServiceAsync(service);

            return new UploadServiceImageResult(blobPath, sas);
        }
    }
}
