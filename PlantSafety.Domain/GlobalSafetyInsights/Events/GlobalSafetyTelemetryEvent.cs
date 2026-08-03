// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights
//  Layer:          Domain
//  File:           GlobalSafetyTelemetryEvent.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §4 Event Mesh Extensions)
//  SchemaVersion:  2026.1
//  Description:    Phase 8 §4 internal cross-site event-mesh extension. Raised by
//                  GlobalSafetyTelemetryConsumer once a real per-site domain event (incident,
//                  telemetry, exposure, production) has been normalized by
//                  IGlobalSafetyTelemetryNormalizer into a canonical SafetyTelemetryEvent -- fans
//                  the fact of ingestion out to any other in-process subscriber (e.g. a future
//                  live-ingestion counter on the dashboard) without re-publishing the full
//                  telemetry payload. Carries TenantId/RegionCode/SiteId plus the standard
//                  Correlation/Causation/UtcTimestamp/SchemaVersion envelope (Handbook §2.17-2.18).
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.GlobalSafetyInsights.ValueObjects.Ids;

namespace PlantSafety.Domain.GlobalSafetyInsights.Events;

/// <summary>Raised when a real per-site event has been normalized into the canonical telemetry shape.</summary>
public sealed record GlobalSafetyTelemetryEvent(
    GlobalSafetyInsightsEventId EventId,
    Guid                        TenantId,
    string                      RegionCode,
    Guid                        SiteId,
    string                      SourceModule,
    UtcTimestamp                IngestedAtUtc,
    PerformedByActor            Actor,
    CorrelationId               CorrelationId,
    CausationId                 CausationId
) : IGlobalSafetyInsightsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "GlobalSafetyTelemetryIngested";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => IngestedAtUtc;
}
