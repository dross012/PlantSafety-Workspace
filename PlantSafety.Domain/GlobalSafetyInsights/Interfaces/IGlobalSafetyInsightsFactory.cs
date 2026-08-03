// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights
//  Layer:          Domain
//  File:           IGlobalSafetyInsightsFactory.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §2 Multi-Site Safety Intelligence Network)
//  SchemaVersion:  2026.1
//  Description:    Factory abstraction for constructing GlobalSafetyInsightsAggregate snapshots,
//                  mirroring RiskManagement.Interfaces.IRiskFactory's convention. Additive-only
//                  evolution.
// ============================================================================
using PlantSafety.Domain.GlobalSafetyInsights.Aggregates;
using PlantSafety.Domain.GlobalSafetyInsights.ValueObjects;
using PlantSafety.SharedKernel.Contracts.Enterprise.MultiRegion;

namespace PlantSafety.Domain.GlobalSafetyInsights.Interfaces;

/// <summary>Factory abstraction for constructing GlobalSafetyInsightsAggregate snapshots.</summary>
public interface IGlobalSafetyInsightsFactory
{
    /// <summary>Constructs and validates a new computed snapshot.</summary>
    GlobalSafetyInsightsAggregate Compute(
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
        PerformedByActor actor);
}
