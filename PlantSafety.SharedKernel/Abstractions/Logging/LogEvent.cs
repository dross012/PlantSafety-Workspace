/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-26
    FILE:               LogEvent.cs
    DESCRIPTION:        Immutable, strong-typed log event envelope. Represents the canonical log shape used across all
                        modules and layers, aligned with the Canonical Log Envelope Rule and Unified Logging Metadata Schema.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only. New fields MAY be added; existing fields MUST remain stable.
==========================================================================================================================*/

using System;

namespace PlantSafety.SharedKernel.Abstractions.Logging;

/// <summary>
/// Immutable canonical log event envelope.
/// </summary>
public sealed record LogEvent(
    LogLevel Level,
    string Message,
    string CorrelationId,
    string? CausationId,
    string Module,
    string Operation,
    string? Actor,
    DateTime UtcTimestamp,
    Exception? Exception);
