/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-26
    FILE:               StorageObjectType.cs
    DESCRIPTION:        Canonical enumeration of storage object types. Required by the Storage Architectural Rule and
                        Canonical Normalization Pipeline.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

namespace PlantSafety.SharedKernel.Abstractions.Storage;

/// <summary>
/// Canonical storage object types.
/// </summary>
public enum StorageObjectType
{
    File = 0,
    Blob = 1,
    Document = 2,
    Image = 3,
    Video = 4
}
