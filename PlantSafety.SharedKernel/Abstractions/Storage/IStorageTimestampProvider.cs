/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-26
    FILE:               IStorageTimestampProvider.cs
    DESCRIPTION:        UTC-only timestamp provider for storage operations. Enforces the Deterministic Time Rule.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using System;

namespace PlantSafety.SharedKernel.Abstractions.Storage;

/// <summary>
/// Provides deterministic UTC timestamps for storage operations.
/// </summary>
public interface IStorageTimestampProvider
{
    DateTime UtcNow { get; }
}
