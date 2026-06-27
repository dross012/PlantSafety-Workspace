/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-26
    FILE:               IStorageObject.cs
    DESCRIPTION:        Strong-typed representation of a stored object. Contains canonical metadata and object type
                        information required by the Storage Architectural Rule.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

namespace PlantSafety.SharedKernel.Abstractions.Storage;

/// <summary>
/// Strong-typed representation of a stored object.
/// </summary>
public interface IStorageObject
{
    string StorageId { get; }
    StorageObjectType ObjectType { get; }
    IStorageMetadata Metadata { get; }
}
