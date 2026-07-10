using Application_Layer.DTOs;
using Application_Layer.Interfaces;

namespace Infrastructure_Layer.Storage;

public sealed class NullFileService : IFileService
{
    public Task<(string blobPath, string sasUrl)> UploadServiceImageAsync(FileUploadRequest file, CancellationToken ct)
    {
        throw new InvalidOperationException("Storage is not configured for service image uploads.");
    }

    public Task<(string blobPath, string sasUrl)> UploadUserAvatarAsync(string userId, FileUploadRequest file, CancellationToken ct)
    {
        throw new InvalidOperationException("Storage is not configured for avatar uploads.");
    }

    public Task<string> GenerateServiceImageReadUrlAsync(string blobPath, CancellationToken ct)
    {
        return Task.FromResult(blobPath);
    }

    public Task DeleteAsync(string blobPath, CancellationToken ct)
    {
        return Task.CompletedTask;
    }
}
