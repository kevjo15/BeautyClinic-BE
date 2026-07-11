using Application_Layer.DTOs;

namespace Application_Layer.Interfaces
{
    public interface IServiceImageUrlResolver
    {
        Task<string> ResolveAsync(string? storedImageUrl, CancellationToken ct);
        Task ApplyAsync(IEnumerable<ServiceDTO> services, CancellationToken ct);
    }
}
