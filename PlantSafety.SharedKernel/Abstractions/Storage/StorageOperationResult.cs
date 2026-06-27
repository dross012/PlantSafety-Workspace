/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-26
    FILE:               StorageOperationResult.cs
    DESCRIPTION:        Strong-typed result for storage operations. Indicates success or failure and includes canonical
                        SIEM-safe error information.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

namespace PlantSafety.SharedKernel.Abstractions.Storage;

/// <summary>
/// Strong-typed result for storage operations.
/// </summary>
public sealed record StorageOperationResult(
    bool Success,
    string? ErrorMessage);
