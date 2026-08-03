// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights
//  Layer:          Domain
//  File:           GlobalSafetyInsightsInvariantRules.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §2 Multi-Site Safety Intelligence Network)
//  SchemaVersion:  2026.1
//  Description:    Pure, stateless invariant checks enforced by
//                  GlobalSafetyInsightsAggregate.Compute before a snapshot is constructed.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.GlobalSafetyInsights.Exceptions;

namespace PlantSafety.Domain.GlobalSafetyInsights.Rules;

/// <summary>Pure invariant checks for GlobalSafetyInsightsAggregate construction.</summary>
public static class GlobalSafetyInsightsInvariantRules
{
    /// <summary>Every snapshot must be scoped to a non-empty TenantId.</summary>
    public static void EnsureTenantIdPresent(Guid tenantId, Guid correlationId, Guid causationId)
    {
        if (tenantId == Guid.Empty)
            throw new GlobalSafetyInsightsInvalidComputationException("TenantId must not be empty.", correlationId, causationId);
    }

    /// <summary>Every snapshot must cover at least one site (Phase 8 is a multi-SITE network, never zero-site).</summary>
    public static void EnsureAtLeastOneSite(IReadOnlyCollection<Guid> siteIds, Guid correlationId, Guid causationId)
    {
        if (siteIds is null || siteIds.Count == 0)
            throw new GlobalSafetyInsightsInvalidComputationException("A snapshot must cover at least one SiteId.", correlationId, causationId);

        if (siteIds.Any(id => id == Guid.Empty))
            throw new GlobalSafetyInsightsInvalidComputationException("SiteIds must not contain an empty GUID.", correlationId, causationId);
    }

    /// <summary>KPI values must be non-negative -- a negative TRIR/LTIR/exposure rate/density indicates a computation bug, never a valid domain state.</summary>
    public static void EnsureKpisAreNonNegative(ValueObjects.GlobalSafetyKpiSnapshot kpis, Guid correlationId, Guid causationId)
    {
        if (kpis.Trir < 0 || kpis.Ltir < 0 || kpis.ExposureRate < 0 || kpis.IncidentDensity < 0 || kpis.TotalHoursWorked < 0)
            throw new GlobalSafetyInsightsInvalidComputationException(
                "Computed KPI values must be non-negative.", correlationId, causationId);

        if (kpis.MachineStateIncidentCorrelation is < -1.0 or > 1.0)
            throw new GlobalSafetyInsightsInvalidComputationException(
                "MachineStateIncidentCorrelation must be within [-1, 1].", correlationId, causationId);
    }
}
