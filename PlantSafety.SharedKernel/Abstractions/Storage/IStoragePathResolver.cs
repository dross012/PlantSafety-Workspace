/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-26
    FILE:               IStoragePathResolver.cs
    DESCRIPTION:        Deterministic path resolver abstraction. Generates canonical, SIEM-safe, normalized storage paths
                        for uploaded objects.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

namespace PlantSafety.SharedKernel.Abstractions.Storage;

/// <summary>
/// Resolves deterministic, normalized storage paths.
/// </summary>
public interface IStoragePathResolver
{
    string ResolvePath(string fileName, StorageObjectType objectType);
}
