using Application_Layer.Interfaces;
using MediatR;

namespace Application_Layer.Features.Services.Commands.UploadServiceImage
{
    public sealed class UploadServiceImageHandler : IRequestHandler<UploadServiceImageCommand, UploadServiceImageResult>
    {
        private readonly IServiceRepository _repo;
        private readonly IFileService _files;

        public UploadServiceImageHandler(IServiceRepository repo, IFileService files)
        {
            _repo = repo;
            _files = files;
        }

        public async Task<UploadServiceImageResult> Handle(UploadServiceImageCommand request, CancellationToken ct)
        {
            var service = await _repo.GetServiceByIdAsync(request.ServiceId)
                ?? throw new KeyNotFoundException("Service not found");

            var (blobPath, sas) = await _files.UploadServiceImageAsync(request.File, ct);

            service.ImageUrl = blobPath;
            await _repo.UpdateServiceAsync(service);

            return new UploadServiceImageResult(blobPath, sas);
        }
    }
}
