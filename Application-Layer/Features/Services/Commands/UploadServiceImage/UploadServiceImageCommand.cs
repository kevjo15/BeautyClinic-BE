using MediatR;
using Application_Layer.DTO_s;

namespace Application_Layer.Features.Services.Commands.UploadServiceImage
{
    public sealed record UploadServiceImageCommand(Guid ServiceId, FileUploadRequest File) : IRequest<UploadServiceImageResult>;

    public sealed record UploadServiceImageResult(string ImageBlobPath, string ImageSasUrl);
}
