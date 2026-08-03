// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights
//  Layer:          Domain
//  File:           GlobalSafetyInsightsSnapshot.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §2 Multi-Site Safety Intelligence Network)
//  SchemaVersion:  2026.1
//  Description:    Immutable snapshot record for fast GlobalSafetyInsightsAggregate summary
//                  projection (mirrors RiskManagement.Snapshots.RiskSnapshot's rehydration-summary
//                  convention). Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.GlobalSafetyInsights.ValueObjects;
using PlantSafety.Domain.GlobalSafetyInsights.ValueObjects.Ids;
using PlantSafety.SharedKernel.Contracts.Enterprise.MultiRegion;

namespace PlantSafety.Domain.GlobalSafetyInsights.Snapshots;

/// <summary>Immutable summary snapshot of a GlobalSafetyInsightsAggregate.</summary>
public sealed record GlobalSafetyInsightsSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public GlobalSafetyInsightsId GlobalSafetyInsightsId { get; init; }

    /// <summary>Owning tenant.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Region this snapshot was computed for.</summary>
    public Region Region { get; init; } = Region.Default;

    /// <summary>Number of sites covered.</summary>
    public int SiteCount { get; init; }

    /// <summary>Computed KPI values at snapshot time.</summary>
    public GlobalSafetyKpiSnapshot Kpis { get; init; } = null!;

    /// <summary>UTC instant this snapshot was computed.</summary>
    public UtcTimestamp ComputedAtUtc { get; init; }
}
