// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights
//  Layer:          Domain
//  File:           GlobalSafetyAnomalyEvent.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §4 Event Mesh Extensions)
//  SchemaVersion:  2026.1
//  Description:    Phase 8 §4 internal cross-site event-mesh contract published by
//                  GlobalSafetyInsightsPublisher/GlobalSafetyDashboardPublisher for every anomaly a
//                  GlobalSafetyInsightsAggregate records, for the Phase 8 dashboard (built by a
//                  later agent) to consume. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.GlobalSafetyInsights.ValueObjects;
using PlantSafety.Domain.GlobalSafetyInsights.ValueObjects.Ids;

namespace PlantSafety.Domain.GlobalSafetyInsights.Events;

/// <summary>Published for downstream dashboard consumption whenever a new anomaly is recorded.</summary>
public sealed record GlobalSafetyAnomalyEvent(
    GlobalSafetyInsightsEventId EventId,
    GlobalSafetyInsightsId      GlobalSafetyInsightsId,
    Guid                        TenantId,
    string                      RegionCode,
    Guid                        SiteId,
    DetectedAnomaly             Anomaly,
    UtcTimestamp                PublishedAtUtc,
    PerformedByActor            Actor,
    CorrelationId               CorrelationId,
    CausationId                 CausationId
) : IGlobalSafetyInsightsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "GlobalSafetyAnomalyPublished";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => PublishedAtUtc;
}
