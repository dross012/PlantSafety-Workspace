// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operations.Configuration
//  Layer:          Application
//  File:           ITenantFeatureFlagOverrideRepository.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Repository abstraction over the real [Release].[TenantFeatureFlagOverride]
//                  table (Phase 7, migration 200-tenant-user-region-feature-flag-overrides.sql,
//                  PlantSafety.ArchiveDB -- co-located with the existing
//                  [System].[FeatureFlag] governance table for the same operational
//                  reasons documented on IFeatureFlagRepository). Additive-only evolution.
// ============================================================================
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using PlantSafety.Application.Operations.Configuration.Models;

namespace PlantSafety.Application.Operations.Configuration.Repositories;

/// <summary>Repository over the real [Release].[TenantFeatureFlagOverride] table.</summary>
public interface ITenantFeatureFlagOverrideRepository
{
    /// <summary>Returns the override for the given tenant/feature pair, or null if none exists.</summary>
    Task<TenantFeatureFlagOverrideDto?> FindAsync(Guid tenantId, string featureName, CancellationToken cancellationToken);

    /// <summary>Returns every override configured for the given feature, across all tenants.</summary>
    Task<IReadOnlyList<TenantFeatureFlagOverrideDto>> GetAllForFeatureAsync(string featureName, CancellationToken cancellationToken);

    /// <summary>Creates or updates (upserts) the override for the given tenant/feature pair.</summary>
    /// <param name="tenantId">The tenant the override applies to.</param>
    /// <param name="featureName">The feature flag name.</param>
    /// <param name="enabled">Whether the feature is enabled for this tenant.</param>
    /// <param name="rolloutPercentage">Optional tenant-scoped rollout percentage (0-100), or null to clear it.</param>
    /// <param name="updatedBy">The identity of the caller making the change.</param>
    Task<TenantFeatureFlagOverrideDto> SetAsync(Guid tenantId, string featureName, bool enabled, int? rolloutPercentage, string updatedBy, CancellationToken cancellationToken);

    /// <summary>
    /// Resets (sets to 0) the rollout percentage of every tenant override configured for the
    /// given feature. Used by rollback automation (<see cref="Release.IRollbackAutomationPolicy"/>)
    /// to revert an in-progress canary rollout without deleting the tenant's on/off override.
    /// </summary>
    Task<int> ResetRolloutPercentageForFeatureAsync(string featureName, string updatedBy, CancellationToken cancellationToken);
}
