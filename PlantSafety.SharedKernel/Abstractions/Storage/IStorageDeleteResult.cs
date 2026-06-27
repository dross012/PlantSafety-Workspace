/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-26
    FILE:               IStorageDeleteResult.cs
    DESCRIPTION:        Strong-typed delete result envelope. Indicates whether the delete operation succeeded and includes
                        canonical metadata for audit and SIEM-safe logging.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

namespace PlantSafety.SharedKernel.Abstractions.Storage;

/// <summary>
/// Strong-typed delete result envelope.
/// </summary>
public interface IStorageDeleteResult
{
    string StorageId { get; }
    bool Deleted { get; }
}
