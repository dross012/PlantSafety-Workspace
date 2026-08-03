// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights
//  Layer:          Domain
//  File:           GlobalSafetyAnomalyDetectedEvent.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §2 Multi-Site Safety Intelligence Network)
//  SchemaVersion:  2026.1
//  Description:    Raised once per DetectedAnomaly recorded on a GlobalSafetyInsightsAggregate
//                  snapshot. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.GlobalSafetyInsights.ValueObjects;
using PlantSafety.Domain.GlobalSafetyInsights.ValueObjects.Ids;

namespace PlantSafety.Domain.GlobalSafetyInsights.Events;

/// <summary>Raised when a cross-site/systemic anomaly is recorded on a snapshot.</summary>
public sealed record GlobalSafetyAnomalyDetectedEvent(
    GlobalSafetyInsightsEventId EventId,
    GlobalSafetyInsightsId      GlobalSafetyInsightsId,
    Guid                        TenantId,
    DetectedAnomaly             Anomaly,
    UtcTimestamp                DetectedAtUtc,
    PerformedByActor            Actor,
    CorrelationId               CorrelationId,
    CausationId                 CausationId
) : IGlobalSafetyInsightsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "GlobalSafetyAnomalyDetected";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => DetectedAtUtc;
}
