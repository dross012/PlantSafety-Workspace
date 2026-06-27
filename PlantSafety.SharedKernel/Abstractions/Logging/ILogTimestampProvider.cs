/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-26
    FILE:               ILogTimestampProvider.cs
    DESCRIPTION:        Abstraction for providing deterministic UTC timestamps for logging. Enforces the Deterministic Time
                        Rule and UTC-only behavior.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using System;

namespace PlantSafety.SharedKernel.Abstractions.Logging;

/// <summary>
/// Provides deterministic UTC timestamps for log events.
/// MUST use UTC and MUST NOT depend on local time or server locale.
/// </summary>
public interface ILogTimestampProvider
{
    DateTime UtcNow { get; }
}
