/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-26
    FILE:               IStorageDownloadResult.cs
    DESCRIPTION:        Strong-typed download result envelope. Contains content, metadata, and canonical storage identifiers.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

namespace PlantSafety.SharedKernel.Abstractions.Storage;

/// <summary>
/// Strong-typed download result envelope.
/// </summary>
public interface IStorageDownloadResult
{
    string StorageId { get; }
    byte[] Content { get; }
    IStorageMetadata Metadata { get; }
}
