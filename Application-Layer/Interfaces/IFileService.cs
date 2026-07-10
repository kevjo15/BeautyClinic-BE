using Application_Layer.DTOs;

namespace Application_Layer.Interfaces
{
    public interface IFileService
    {
        Task<(string blobPath, string sasUrl)> UploadServiceImageAsync(FileUploadRequest file, CancellationToken ct);
        Task<(string blobPath, string sasUrl)> UploadUserAvatarAsync(string userId, FileUploadRequest file, CancellationToken ct);
        Task<string> GenerateServiceImageReadUrlAsync(string blobPath, CancellationToken ct);
        Task DeleteAsync(string blobPath, CancellationToken ct);
    }
}
