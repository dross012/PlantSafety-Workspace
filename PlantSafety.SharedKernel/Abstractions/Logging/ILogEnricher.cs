/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-26
    FILE:               ILogEnricher.cs
    DESCRIPTION:        Abstraction for enriching log events with additional deterministic, SIEM-safe metadata. Used by
                        infrastructure implementations to attach module, environment, and operation context.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

namespace PlantSafety.SharedKernel.Abstractions.Logging;

/// <summary>
/// Enriches log events with additional metadata (module, environment, operation, etc.).
/// Implementations MUST be deterministic and SIEM-safe.
/// </summary>
public interface ILogEnricher
{
    LogEvent Enrich(LogEvent logEvent);
}
