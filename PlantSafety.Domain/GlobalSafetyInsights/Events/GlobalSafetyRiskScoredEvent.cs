// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights
//  Layer:          Domain
//  File:           GlobalSafetyRiskScoredEvent.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §2 Multi-Site Safety Intelligence Network)
//  SchemaVersion:  2026.1
//  Description:    Raised once per SiteRiskScore recorded on a GlobalSafetyInsightsAggregate
//                  snapshot. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.GlobalSafetyInsights.ValueObjects;
using PlantSafety.Domain.GlobalSafetyInsights.ValueObjects.Ids;

namespace PlantSafety.Domain.GlobalSafetyInsights.Events;

/// <summary>Raised when a per-site/region/machine-family risk score is recorded on a snapshot.</summary>
public sealed record GlobalSafetyRiskScoredEvent(
    GlobalSafetyInsightsEventId EventId,
    GlobalSafetyInsightsId      GlobalSafetyInsightsId,
    Guid                        TenantId,
    SiteRiskScore               RiskScore,
    UtcTimestamp                ScoredAtUtc,
    PerformedByActor            Actor,
    CorrelationId               CorrelationId,
    CausationId                 CausationId
) : IGlobalSafetyInsightsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "GlobalSafetyRiskScored";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ScoredAtUtc;
}
