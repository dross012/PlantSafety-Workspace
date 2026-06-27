/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-26
    FILE:               IStorageClient.cs
    DESCRIPTION:        Canonical storage client abstraction. Provides deterministic, SIEM-safe, strong-typed operations
                        for uploading, downloading, deleting, and retrieving metadata for stored objects. This interface is
                        infrastructure-agnostic and MUST NOT leak cloud provider or filesystem details.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

namespace PlantSafety.SharedKernel.Abstractions.Storage;

/// <summary>
/// Canonical storage client abstraction.
/// MUST be deterministic, SIEM-safe, and infrastructure-agnostic.
/// </summary>
public interface IStorageClient
{
    StorageOperationResult Upload(IStorageUploadRequest request);
    IStorageDownloadResult Download(string storageId);
    IStorageDeleteResult Delete(string storageId);
    IStorageMetadata GetMetadata(string storageId);
}
