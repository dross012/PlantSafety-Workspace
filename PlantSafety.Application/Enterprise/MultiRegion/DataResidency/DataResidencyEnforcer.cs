// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Application.Enterprise.MultiRegion.DataResidency
//  Layer:          Application
//  File:           DataResidencyEnforcer.cs
//  Created:        2026-08-03
//  Author:         Security & Compliance Agent (Phase 7 §5.3)
//  SchemaVersion:  2026.1
//  Description:    Default IDataResidencyEnforcer: compares a tenant's configured
//                  DataResidencyPolicy (ITenantDataResidencyProfileStore) against the platform's
//                  current deployment region (ICurrentDeploymentRegionProvider). Pure,
//                  deterministic, fully unit-testable without any database/network dependency.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.MultiRegion.DataResidency;
using PlantSafety.SharedKernel.Identifiers;

namespace PlantSafety.Application.Enterprise.MultiRegion.DataResidency;

/// <summary>Default <see cref="IDataResidencyEnforcer"/> implementation.</summary>
public sealed class DataResidencyEnforcer(
    ITenantDataResidencyProfileStore profileStore,
    ICurrentDeploymentRegionProvider currentDeploymentRegionProvider) : IDataResidencyEnforcer
{
    /// <inheritdoc/>
    public bool IsSatisfied(Guid tenantId)
    {
        var profile = profileStore.GetProfile(tenantId);
        var currentRegion = currentDeploymentRegionProvider.CurrentRegion;

        return profile.Policy.IsSatisfiedByZone(currentRegion.DataResidencyZone);
    }

    /// <inheritdoc/>
    public void EnsureSatisfied(Guid tenantId, CorrelationId correlationId, CausationId causationId)
    {
        var profile = profileStore.GetProfile(tenantId);
        var currentRegion = currentDeploymentRegionProvider.CurrentRegion;

        if (!profile.Policy.IsSatisfiedByZone(currentRegion.DataResidencyZone))
        {
            throw new DataResidencyViolationException(tenantId, profile.Policy, currentRegion, correlationId, causationId);
        }
    }
}
