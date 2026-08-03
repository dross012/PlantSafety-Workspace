// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Application.Common.Rollout
//  Layer:          Application
//  File:           ITenantRolloutPolicy.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Phase 6.6 (§6.2 Blue/Green + Canary + Tenant Rollout) abstraction over
//                  "is this tenant in the rollout wave for this feature". Sits alongside
//                  (not instead of) the existing binary IFeatureFlagRepository/FeatureFlagDto
//                  IsEnabled switch -- IsEnabled gates the feature on/off globally;
//                  ITenantRolloutPolicy additionally gates it per-tenant once a
//                  RolloutPercentage is configured. Pure/deterministic by contract: the same
//                  (tenantId, featureName, rolloutPercentage) input MUST always produce the
//                  same answer, so canary membership is stable across calls/replicas/restarts
//                  (not randomly re-decided per call). Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Common.Rollout;

/// <summary>
/// Deterministically decides whether a given tenant is in the canary-rollout wave for a
/// given feature/percentage combination.
/// </summary>
public interface ITenantRolloutPolicy
{
    /// <summary>
    /// Returns <see langword="true"/> if <paramref name="tenantId"/> falls within the
    /// rollout wave for <paramref name="featureName"/> at <paramref name="rolloutPercentage"/>.
    /// Deterministic: the same inputs always produce the same result.
    /// </summary>
    /// <param name="tenantId">The tenant to evaluate.</param>
    /// <param name="featureName">The feature flag name.</param>
    /// <param name="rolloutPercentage">
    /// The configured rollout percentage (0-100). A value of 0 always returns
    /// <see langword="false"/>; a value of 100 always returns <see langword="true"/>.
    /// </param>
    bool IsInRollout(Guid tenantId, string featureName, int rolloutPercentage);
}
