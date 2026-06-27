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
}
