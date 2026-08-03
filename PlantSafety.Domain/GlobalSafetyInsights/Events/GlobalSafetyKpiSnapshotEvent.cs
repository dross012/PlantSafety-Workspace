// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights
//  Layer:          Domain
//  File:           GlobalSafetyKpiSnapshotEvent.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §4 Event Mesh Extensions)
//  SchemaVersion:  2026.1
//  Description:    Phase 8 §4 internal cross-site event-mesh contract published by
//                  GlobalSafetyInsightsPublisher/GlobalSafetyDashboardPublisher whenever a
//                  GlobalSafetyInsightsAggregate finishes computing a new KPI snapshot, for the
//                  Phase 8 dashboard (built by a later agent) to consume. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.GlobalSafetyInsights.ValueObjects;
using PlantSafety.Domain.GlobalSafetyInsights.ValueObjects.Ids;

namespace PlantSafety.Domain.GlobalSafetyInsights.Events;

/// <summary>Published for downstream dashboard consumption whenever a new KPI snapshot is computed.</summary>
public sealed record GlobalSafetyKpiSnapshotEvent(
    GlobalSafetyInsightsEventId EventId,
    GlobalSafetyInsightsId      GlobalSafetyInsightsId,
    Guid                        TenantId,
    string                      RegionCode,
    Guid                        SiteId,
    SafetyAnalysisTimeWindow    TimeWindow,
    GlobalSafetyKpiSnapshot     Kpis,
    UtcTimestamp                PublishedAtUtc,
    PerformedByActor            Actor,
    CorrelationId               CorrelationId,
    CausationId                 CausationId
) : IGlobalSafetyInsightsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "GlobalSafetyKpiSnapshotPublished";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => PublishedAtUtc;
}
