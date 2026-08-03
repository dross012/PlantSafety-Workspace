// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights
//  Layer:          Domain
//  File:           GlobalSafetyInsightComputedEvent.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §2 Multi-Site Safety Intelligence Network)
//  SchemaVersion:  2026.1
//  Description:    Raised when a GlobalSafetyInsightsAggregate finishes computing a new snapshot
//                  (KPIs + risk scores + anomalies + recommendations) for an analysis window.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.GlobalSafetyInsights.ValueObjects;
using PlantSafety.Domain.GlobalSafetyInsights.ValueObjects.Ids;

namespace PlantSafety.Domain.GlobalSafetyInsights.Events;

/// <summary>Raised when a new GlobalSafetyInsights snapshot has been computed.</summary>
public sealed record GlobalSafetyInsightComputedEvent(
    GlobalSafetyInsightsEventId EventId,
    GlobalSafetyInsightsId      GlobalSafetyInsightsId,
    Guid                        TenantId,
    string                      RegionCode,
    SafetyAnalysisTimeWindow    TimeWindow,
    int                         SiteCount,
    int                         AnomalyCount,
    int                         RecommendationCount,
    UtcTimestamp                ComputedAtUtc,
    PerformedByActor            Actor,
    CorrelationId               CorrelationId,
    CausationId                 CausationId
) : IGlobalSafetyInsightsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "GlobalSafetyInsightComputed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ComputedAtUtc;
}
