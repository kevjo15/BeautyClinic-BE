using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application_Layer.Features.Services.Commands.UploadServiceImage
{
    public sealed record UploadServiceImageCommand(Guid ServiceId, IFormFile File) : IRequest<UploadServiceImageResult>;

    public sealed record UploadServiceImageResult(string ImageBlobPath, string ImageSasUrl);
}
