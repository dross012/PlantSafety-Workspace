/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-26
    FILE:               IStorageUploadRequest.cs
    DESCRIPTION:        Canonical upload request envelope. Contains strong-typed metadata, object type, and deterministic
                        storage path information.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

namespace PlantSafety.SharedKernel.Abstractions.Storage;

/// <summary>
/// Strong-typed upload request envelope.
/// </summary>
public interface IStorageUploadRequest
{
    string FileName { get; }
    byte[] Content { get; }
    StorageObjectType ObjectType { get; }
    string DeterministicPath { get; }
}
