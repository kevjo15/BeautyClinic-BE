using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Application_Layer.Services
{
    public class FileService : IFileService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly IConfiguration _configuration;
        private readonly bool _useAzurite;

        public FileService(IConfiguration configuration, BlobServiceClient blobServiceClient)
        {
            _configuration = configuration;
            _blobServiceClient = blobServiceClient;
            _useAzurite = _configuration.GetValue<bool>("Storage:UseAzurite", false);
        }

        public async Task<(string blobPath, string sasUrl)> UploadAsync(IFormFile file, string container, TimeSpan sasLifetime, CancellationToken ct)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(container);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: ct);

            var blobName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var blobClient = containerClient.GetBlobClient(blobName);

            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, true, ct);
                await blobClient.SetHttpHeadersAsync(new BlobHttpHeaders { ContentType = file.ContentType }, cancellationToken: ct);
            }

            var sasUrl = await GenerateReadSasAsync(container, blobName, sasLifetime, ct);
            return ($"{container}/{blobName}", sasUrl);
        }

        public async Task<string> GenerateReadSasAsync(string container, string blobPath, TimeSpan sasLifetime, CancellationToken ct)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(container);
            var blobClient = containerClient.GetBlobClient(blobPath);

            var builder = new BlobSasBuilder
            {
                BlobContainerName = container,
                BlobName = blobPath,
                Resource = "b",
                ExpiresOn = DateTimeOffset.UtcNow.Add(sasLifetime),
                Protocol = _useAzurite ? SasProtocol.HttpsAndHttp : SasProtocol.Https
            };
            builder.SetPermissions(BlobSasPermissions.Read);

            if (blobClient.CanGenerateSasUri)
            {
                return blobClient.GenerateSasUri(builder).ToString();
            }

            var userDelegationKey = await _blobServiceClient.GetUserDelegationKeyAsync(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.Add(sasLifetime), ct);
            var sasToken = builder.ToSasQueryParameters(userDelegationKey, _blobServiceClient.AccountName).ToString();

            return $"{blobClient.Uri}?{sasToken}";
        }
    }
}
