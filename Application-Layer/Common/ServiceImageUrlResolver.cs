using Application_Layer.DTOs;
using Application_Layer.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application_Layer.Common
{
    /// <summary>
    /// Turns stored image references ("&lt;container&gt;/&lt;blobName&gt;") into short-lived read URLs.
    /// The database never stores SAS URLs; they are generated fresh on every read.
    /// </summary>
    public sealed class ServiceImageUrlResolver : IServiceImageUrlResolver
    {
        private readonly IFileService _files;
        private readonly ILogger<ServiceImageUrlResolver> _logger;

        public ServiceImageUrlResolver(IFileService files, ILogger<ServiceImageUrlResolver> logger)
        {
            _files = files;
            _logger = logger;
        }

        public async Task<string> ResolveAsync(string? storedImageUrl, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(storedImageUrl))
            {
                return string.Empty;
            }

            // Legacy rows may contain a full URL instead of a blob path.
            if (storedImageUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                storedImageUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return storedImageUrl;
            }

            try
            {
                return await _files.GenerateServiceImageReadUrlAsync(storedImageUrl, ct);
            }
            catch (Exception ex)
            {
                // A single bad image reference must not fail the whole request.
                _logger.LogWarning(ex, "Could not resolve image URL for {ImageUrl}", storedImageUrl);
                return string.Empty;
            }
        }

        public async Task ApplyAsync(IEnumerable<ServiceDTO> services, CancellationToken ct)
        {
            foreach (var service in services)
            {
                service.ImageUrl = await ResolveAsync(service.ImageUrl, ct);
            }
        }
    }
}
