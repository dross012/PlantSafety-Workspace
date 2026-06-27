/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-26
    FILE:               IStorageIdProvider.cs
    DESCRIPTION:        UUIDv7 identifier generator for stored objects. Enforces the Deterministic Identifier Rule.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

namespace PlantSafety.SharedKernel.Abstractions.Storage;

/// <summary>
/// Generates deterministic UUIDv7 identifiers for stored objects.
/// </summary>
public interface IStorageIdProvider
{
    string CreateStorageId();
}
