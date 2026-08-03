// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights
//  Layer:          Domain
//  File:           GlobalSafetyInsightsFactory.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §2 Multi-Site Safety Intelligence Network)
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing GlobalSafetyInsightsAggregate
//                  snapshots. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.GlobalSafetyInsights.Aggregates;
using PlantSafety.Domain.GlobalSafetyInsights.Interfaces;
using PlantSafety.Domain.GlobalSafetyInsights.ValueObjects;
using PlantSafety.SharedKernel.Contracts.Enterprise.MultiRegion;

namespace PlantSafety.Domain.GlobalSafetyInsights.Factories;

/// <summary>Default factory implementation for constructing GlobalSafetyInsightsAggregate snapshots.</summary>
public sealed class GlobalSafetyInsightsFactory : IGlobalSafetyInsightsFactory
{
    /// <inheritdoc/>
    public GlobalSafetyInsightsAggregate Compute(
        Guid tenantId,
        Region region,
        IReadOnlyList<Guid> siteIds,
        SafetyAnalysisTimeWindow timeWindow,
        GlobalSafetyKpiSnapshot kpis,
        IReadOnlyList<SiteRiskScore> riskScores,
        IReadOnlyList<DetectedAnomaly> anomalies,
        IReadOnlyList<SafetyRecommendation> recommendations,
        UserId createdBy,
        UtcTimestamp nowUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => GlobalSafetyInsightsAggregate.Compute(
            tenantId, region, siteIds, timeWindow, kpis, riskScores, anomalies, recommendations,
            createdBy, nowUtc, correlationId, causationId, actor);
}
