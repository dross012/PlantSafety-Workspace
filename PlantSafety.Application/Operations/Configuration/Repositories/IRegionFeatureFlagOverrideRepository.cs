// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operations.Configuration
//  Layer:          Application
//  File:           IRegionFeatureFlagOverrideRepository.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Repository abstraction over the real [Release].[RegionFeatureFlagOverride]
//                  table (Phase 7, migration 200-tenant-user-region-feature-flag-overrides.sql,
//                  PlantSafety.ArchiveDB). Additive-only evolution.
// ============================================================================
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using PlantSafety.Application.Operations.Configuration.Models;

namespace PlantSafety.Application.Operations.Configuration.Repositories;

/// <summary>Repository over the real [Release].[RegionFeatureFlagOverride] table.</summary>
public interface IRegionFeatureFlagOverrideRepository
{
    /// <summary>Returns the override for the given region/feature pair, or null if none exists.</summary>
    Task<RegionFeatureFlagOverrideDto?> FindAsync(string region, string featureName, CancellationToken cancellationToken);

    /// <summary>Returns every override configured for the given feature, across all regions.</summary>
    Task<IReadOnlyList<RegionFeatureFlagOverrideDto>> GetAllForFeatureAsync(string featureName, CancellationToken cancellationToken);

    /// <summary>Creates or updates (upserts) the override for the given region/feature pair.</summary>
    /// <param name="region">The region the override applies to (e.g. "EU", "US", "APAC").</param>
    /// <param name="featureName">The feature flag name.</param>
    /// <param name="enabled">Whether the feature is enabled in this region.</param>
    /// <param name="rolloutPercentage">Optional region-scoped rollout percentage (0-100), or null to clear it.</param>
    /// <param name="updatedBy">The identity of the caller making the change.</param>
    Task<RegionFeatureFlagOverrideDto> SetAsync(string region, string featureName, bool enabled, int? rolloutPercentage, string updatedBy, CancellationToken cancellationToken);

    /// <summary>
    /// Resets (sets to 0) the rollout percentage of every region override configured for the
    /// given feature. Used by rollback automation to revert an in-progress canary rollout.
    /// </summary>
    Task<int> ResetRolloutPercentageForFeatureAsync(string featureName, string updatedBy, CancellationToken cancellationToken);
}
