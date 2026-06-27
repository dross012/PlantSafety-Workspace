/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-26
    FILE:               ILogContext.cs
    DESCRIPTION:        Canonical log context abstraction. Represents the strong-typed, SIEM-safe context envelope for all
                        log events, including correlation, causation, module, operation, and actor identity.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only. New properties MAY be added; existing properties MUST remain stable.
==========================================================================================================================*/

using System;

namespace PlantSafety.SharedKernel.Abstractions.Logging;

/// <summary>
/// Strong-typed log context envelope used to carry correlation, causation, module, operation, and actor identity.
/// This interface is implemented by infrastructure-specific context providers.
/// </summary>
public interface ILogContext
{
    string CorrelationId { get; }
    string? CausationId { get; }
    string Module { get; }
    string Operation { get; }
    string? Actor { get; }
    DateTime UtcTimestamp { get; }
}
