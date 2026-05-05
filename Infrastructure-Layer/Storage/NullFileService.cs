using Application_Layer.DTOs;
using Application_Layer.Interfaces;

namespace Infrastructure_Layer.Storage;

public sealed class NullFileService : IFileService
{
    public Task<(string blobPath, string sasUrl)> UploadServiceImageAsync(FileUploadRequest file, CancellationToken ct)
    {
        throw new InvalidOperationException("Storage is not configured for service image uploads.");
    }

    public Task<string> GenerateServiceImageReadUrlAsync(string blobPath, CancellationToken ct)
    {
        return Task.FromResult(blobPath);
    }
}
