// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operations.Configuration
//  Layer:          Application
//  File:           IUserFeatureFlagOverrideRepository.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Repository abstraction over the real [Release].[UserFeatureFlagOverride]
//                  table (Phase 7, migration 200-tenant-user-region-feature-flag-overrides.sql,
//                  PlantSafety.ArchiveDB). Additive-only evolution.
// ============================================================================
using System;
using System.Threading;
using System.Threading.Tasks;

using PlantSafety.Application.Operations.Configuration.Models;

namespace PlantSafety.Application.Operations.Configuration.Repositories;

/// <summary>Repository over the real [Release].[UserFeatureFlagOverride] table.</summary>
public interface IUserFeatureFlagOverrideRepository
{
    /// <summary>Returns the override for the given user/feature pair, or null if none exists.</summary>
    Task<UserFeatureFlagOverrideDto?> FindAsync(Guid userId, string featureName, CancellationToken cancellationToken);

    /// <summary>Creates or updates (upserts) the override for the given user/feature pair.</summary>
    /// <param name="userId">The user the override applies to.</param>
    /// <param name="featureName">The feature flag name.</param>
    /// <param name="enabled">Whether the feature is enabled for this specific user.</param>
    /// <param name="updatedBy">The identity of the caller making the change.</param>
    Task<UserFeatureFlagOverrideDto> SetAsync(Guid userId, string featureName, bool enabled, string updatedBy, CancellationToken cancellationToken);
}
