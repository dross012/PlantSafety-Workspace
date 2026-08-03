// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.FileStorage
//  Layer:          Infrastructure
//  File:           AzureBlobStorageService.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    IBlobStorageService backed by Azure Blob Storage SDK.
//                  Additive-only evolution.
// ============================================================================
using Azure.Storage.Blobs;

namespace PlantSafety.Infrastructure.FileStorage;

/// <summary>
/// <see cref="IBlobStorageService"/> backed by the Azure Blob Storage SDK.
/// Uses Managed Identity in production; falls back to connection string in development.
/// </summary>
public sealed class AzureBlobStorageService(
    BlobServiceClient blobServiceClient,
    IOptions<BlobStorageOptions> opts,
    ILogger<AzureBlobStorageService> logger)
    : IBlobStorageService
{
    /// <inheritdoc/>
    public async Task<string> UploadAsync(
        string containerName, string blobName, Stream content, string contentType,
        CancellationToken cancellationToken = default)
    {
        var container = blobServiceClient.GetBlobContainerClient(containerName);
        await container.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        var blob = container.GetBlobClient(blobName);
        await blob.UploadAsync(content, overwrite: true, cancellationToken: cancellationToken);

        logger.LogInformation("Uploaded blob {BlobName} to container {Container}", blobName, containerName);
        return blob.Uri.ToString();
    }

    /// <inheritdoc/>
    public async Task<Stream> DownloadAsync(string containerName, string blobName, CancellationToken cancellationToken = default)
    {
        var blob = blobServiceClient.GetBlobContainerClient(containerName).GetBlobClient(blobName);
        var response = await blob.DownloadStreamingAsync(cancellationToken: cancellationToken);
        return response.Value.Content;
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(string containerName, string blobName, CancellationToken cancellationToken = default)
    {
        var blob = blobServiceClient.GetBlobContainerClient(containerName).GetBlobClient(blobName);
        await blob.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(string containerName, string blobName, CancellationToken cancellationToken = default)
    {
        var blob = blobServiceClient.GetBlobContainerClient(containerName).GetBlobClient(blobName);
        return await blob.ExistsAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<long> GetContainerSizeBytesAsync(string containerName, CancellationToken cancellationToken = default)
    {
        var container = blobServiceClient.GetBlobContainerClient(containerName);
        if (!await container.ExistsAsync(cancellationToken))
            return 0L;

        long totalBytes = 0;
        await foreach (var blobItem in container.GetBlobsAsync(cancellationToken: cancellationToken))
            totalBytes += blobItem.Properties.ContentLength ?? 0L;

        return totalBytes;
    }
}
