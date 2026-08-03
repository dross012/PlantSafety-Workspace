// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.FileStorage
//  Layer:          Infrastructure
//  File:           IBlobStorageService.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Abstraction over Azure Blob Storage for document and evidence file storage.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.FileStorage;

/// <summary>Abstraction over Azure Blob Storage for document and evidence file uploads/downloads.</summary>
public interface IBlobStorageService
{
    /// <summary>Uploads a stream to Blob Storage and returns the canonical blob URI.</summary>
    Task<string> UploadAsync(string containerName, string blobName, Stream content, string contentType, CancellationToken cancellationToken = default);

    /// <summary>Downloads a blob as a stream.</summary>
    Task<Stream> DownloadAsync(string containerName, string blobName, CancellationToken cancellationToken = default);

    /// <summary>Deletes a blob. No-op if not found.</summary>
    Task DeleteAsync(string containerName, string blobName, CancellationToken cancellationToken = default);

    /// <summary>Returns <see langword="true"/> when the blob exists.</summary>
    Task<bool> ExistsAsync(string containerName, string blobName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the total size, in bytes, of every blob currently stored in <paramref name="containerName"/>.
    /// Returns 0 if the container does not exist or is empty. Added for Phase 7 §3.1 Usage
    /// Metering (per-tenant storage-usage billing dimension) -- previously this interface had no
    /// way to answer "how much storage is a tenant using" at all.
    /// </summary>
    Task<long> GetContainerSizeBytesAsync(string containerName, CancellationToken cancellationToken = default);
}
