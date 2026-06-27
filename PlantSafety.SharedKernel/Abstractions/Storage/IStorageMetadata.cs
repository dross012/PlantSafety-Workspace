/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-26
    FILE:               IStorageMetadata.cs
    DESCRIPTION:        Canonical metadata envelope for stored objects. Includes timestamps, size, content type, and
                        SIEM-safe identifiers.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using System;

namespace PlantSafety.SharedKernel.Abstractions.Storage;

/// <summary>
/// Canonical metadata envelope for stored objects.
/// </summary>
public interface IStorageMetadata
{
    string StorageId { get; }
    string FileName { get; }
    string ContentType { get; }
    long SizeBytes { get; }
    DateTime UploadedUtc { get; }
}
