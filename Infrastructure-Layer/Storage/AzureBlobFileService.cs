using Application_Layer.DTOs;
using Application_Layer.Interfaces;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure_Layer.Storage;

public sealed class AzureBlobFileService : IFileService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AzureBlobFileService> _logger;
    private readonly IMemoryCache _cache;

    public AzureBlobFileService(
        BlobServiceClient blobServiceClient,
        IConfiguration configuration,
        ILogger<AzureBlobFileService> logger,
        IMemoryCache cache)
    {
        _blobServiceClient = blobServiceClient;
        _configuration = configuration;
        _logger = logger;
        _cache = cache;
    }

    private static readonly HashSet<string> AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private static readonly HashSet<string> AllowedContentTypes = ["image/jpeg", "image/png", "image/webp"];

    public async Task<(string blobPath, string sasUrl)> UploadServiceImageAsync(FileUploadRequest file, CancellationToken ct)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext) || !AllowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
        {
            _logger.LogWarning("Rejected upload for unsupported file type: {FileName} ({ContentType})", file.FileName, file.ContentType);
            throw new InvalidOperationException("Only image files (jpg, png, webp) are allowed.");
        }

        var container = GetContainerName();
        var containerClient = _blobServiceClient.GetBlobContainerClient(container);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: ct);

        var blobName = $"{Guid.NewGuid()}{ext}";
        var blobClient = containerClient.GetBlobClient(blobName);

        try
        {
            await using var stream = new MemoryStream(file.Content, writable: false);
            await blobClient.UploadAsync(stream, overwrite: true, cancellationToken: ct);
            await blobClient.SetHttpHeadersAsync(
                new BlobHttpHeaders { ContentType = file.ContentType },
                cancellationToken: ct);

            var blobPath = $"{container}/{blobName}";
            var sasUrl = await GenerateServiceImageReadUrlAsync(blobPath, ct);

            _logger.LogInformation("Uploaded service image to {BlobPath} ({SizeBytes} bytes)", blobPath, file.Content.Length);
            return (blobPath, sasUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload service image to container {Container}", container);
            throw;
        }
    }

    public Task<string> GenerateServiceImageReadUrlAsync(string blobPath, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(blobPath);

        var cacheKey = $"sas:{blobPath}";
        if (_cache.TryGetValue(cacheKey, out string? cached) && cached is not null)
            return Task.FromResult(cached);

        try
        {
            var (containerName, blobName) = ParseBlobPath(blobPath);
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            var lifetimeMinutes = GetSasLifetimeMinutes();
            var builder = new BlobSasBuilder
            {
                BlobContainerName = containerName,
                BlobName = blobName,
                Resource = "b",
                ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(lifetimeMinutes),
                Protocol = SasProtocol.HttpsAndHttp
            };

            builder.SetPermissions(BlobSasPermissions.Read);
            var sasUrl = blobClient.GenerateSasUri(builder).ToString();

            _cache.Set(cacheKey, sasUrl, TimeSpan.FromMinutes(lifetimeMinutes / 2));

            return Task.FromResult(sasUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate SAS URL for blob path {BlobPath}", blobPath);
            throw;
        }
    }

    private string GetContainerName()
    {
        return _configuration["Storage:ServiceImagesContainer"] ?? "service-images";
    }

    private int GetSasLifetimeMinutes()
    {
        return _configuration.GetValue<int>("Storage:SasExpiryMinutes", 60);
    }

    private static (string Container, string BlobName) ParseBlobPath(string blobPath)
    {
        var separatorIndex = blobPath.IndexOf('/');
        if (separatorIndex <= 0 || separatorIndex == blobPath.Length - 1)
        {
            throw new InvalidOperationException("Blob path must be stored as '<container>/<blobName>'.");
        }

        return (blobPath[..separatorIndex], blobPath[(separatorIndex + 1)..]);
    }
}
