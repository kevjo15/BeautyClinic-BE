using MediatR;
using Application_Layer.DTOs;

namespace Application_Layer.Commands.ServiceCommands.UploadServiceImage
{
    public sealed record UploadServiceImageCommand(Guid ServiceId, FileUploadRequest File) : IRequest<UploadServiceImageResult>;

    public sealed record UploadServiceImageResult(string ImageBlobPath, string ImageSasUrl);
}
