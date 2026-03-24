using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Application_Layer.Interfaces
{
    public interface IFileService
    {
        Task<(string blobPath, string sasUrl)> UploadAsync(IFormFile file, string container, TimeSpan sasLifetime, CancellationToken ct);
        Task<string> GenerateReadSasAsync(string container, string blobPath, TimeSpan sasLifetime, CancellationToken ct);
    }
}
