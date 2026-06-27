/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-27
    FILE:               EventMetadataConstants.cs
    DESCRIPTION:        Canonical constants for event metadata fields. Used by SafetyAnalyzer, SafetyGenerator, and
                        observability pipelines to enforce deterministic naming.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

namespace PlantSafety.SharedKernel.Events.Metadata;

/// <summary>
/// Canonical constants for event metadata.
/// </summary>
public static class EventMetadataConstants
{
    public const string EventId = "event_id";
    public const string CorrelationId = "correlation_id";
    public const string CausationId = "causation_id";
    public const string Module = "module";
    public const string Actor = "actor";
    public const string OccurredUtc = "occurred_utc";
}
