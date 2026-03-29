using Application_Layer.DTO_s;
using Application_Layer.Interfaces;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Configuration;

namespace Infrastructure_Layer.Storage;

public sealed class AzureBlobFileService : IFileService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly IConfiguration _configuration;

    public AzureBlobFileService(
        BlobServiceClient blobServiceClient,
        IConfiguration configuration)
    {
        _blobServiceClient = blobServiceClient;
        _configuration = configuration;
    }

    public async Task<(string blobPath, string sasUrl)> UploadServiceImageAsync(FileUploadRequest file, CancellationToken ct)
    {
        var container = GetContainerName();
        var containerClient = _blobServiceClient.GetBlobContainerClient(container);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: ct);

        var blobName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var blobClient = containerClient.GetBlobClient(blobName);

        await using var stream = new MemoryStream(file.Content, writable: false);
        await blobClient.UploadAsync(stream, overwrite: true, cancellationToken: ct);
        await blobClient.SetHttpHeadersAsync(
            new BlobHttpHeaders { ContentType = file.ContentType },
            cancellationToken: ct);

        var blobPath = $"{container}/{blobName}";
        var sasUrl = await GenerateServiceImageReadUrlAsync(blobPath, ct);

        return (blobPath, sasUrl);
    }

    public Task<string> GenerateServiceImageReadUrlAsync(string blobPath, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(blobPath);

        var (containerName, blobName) = ParseBlobPath(blobPath);
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        var builder = new BlobSasBuilder
        {
            BlobContainerName = containerName,
            BlobName = blobName,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(GetSasLifetimeMinutes()),
            Protocol = SasProtocol.HttpsAndHttp
        };

        builder.SetPermissions(BlobSasPermissions.Read);
        return Task.FromResult(blobClient.GenerateSasUri(builder).ToString());
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
