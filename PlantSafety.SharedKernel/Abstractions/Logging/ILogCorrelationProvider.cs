/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-26
    FILE:               ILogCorrelationProvider.cs
    DESCRIPTION:        Abstraction for providing correlation and causation identifiers for logging and observability
                        pipelines. Enforces deterministic, SIEM-safe correlation behavior.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

namespace PlantSafety.SharedKernel.Abstractions.Logging;

/// <summary>
/// Provides correlation and causation identifiers for log events.
/// Implementations MUST follow the Canonical Correlation &amp; Causality Lineage Rule.
/// </summary>
public interface ILogCorrelationProvider
{
    string GetCorrelationId();
    string? GetCausationId();
}
