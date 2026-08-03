// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operations.Configuration
//  Layer:          Application
//  File:           TenantFeatureFlagOverrideDto.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Phase 7 (SaaS Productization Blueprint §7.1 Feature Flag System)
//                  tenant-level feature flag override. Sits above the tenant-agnostic
//                  global [System].[FeatureFlag] (FeatureFlagDto) in the resolution
//                  precedence chain: a tenant-level override, when present, takes
//                  precedence over the global flag for that tenant only. See
//                  Resolution\FeatureFlagResolver for the full precedence composition
//                  (user > tenant > region > global). Additive-only evolution.
// ============================================================================
using System;

namespace PlantSafety.Application.Operations.Configuration.Models;

/// <summary>
/// A tenant-specific override of a feature flag, taking precedence over the global flag
/// (but not over a <see cref="UserFeatureFlagOverrideDto"/>) for the given tenant.
/// </summary>
/// <param name="TenantId">The tenant this override applies to.</param>
/// <param name="FeatureName">The feature flag name (matches <see cref="FeatureFlagDto.FlagName"/>).</param>
/// <param name="Enabled">
/// Whether the feature is enabled for this tenant. When <see langword="false"/>, this is an
/// explicit tenant-level kill-switch regardless of <see cref="RolloutPercentage"/> or the
/// global flag's state.
/// </param>
/// <param name="RolloutPercentage">
/// Optional tenant-scoped canary-rollout percentage (0-100), evaluated only when
/// <see cref="Enabled"/> is <see langword="true"/>. NULL means "fully enabled for this
/// tenant, no percentage gating" once <see cref="Enabled"/> is <see langword="true"/>.
/// </param>
/// <param name="UpdatedUtc">When this override was last changed.</param>
/// <param name="UpdatedBy">Who last changed this override.</param>
public sealed record TenantFeatureFlagOverrideDto(
    Guid TenantId,
    string FeatureName,
    bool Enabled,
    int? RolloutPercentage,
    DateTime UpdatedUtc,
    string UpdatedBy);
