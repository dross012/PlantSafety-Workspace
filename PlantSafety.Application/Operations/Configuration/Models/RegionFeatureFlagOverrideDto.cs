// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operations.Configuration
//  Layer:          Application
//  File:           RegionFeatureFlagOverrideDto.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Phase 7 (SaaS Productization Blueprint §7.1) region-level feature flag
//                  override -- e.g. a feature only live in EU first. Sits between tenant-level
//                  overrides and the global flag in the resolution precedence chain
//                  (user > tenant > region > global). See Resolution\FeatureFlagResolver.
//                  Additive-only evolution.
// ============================================================================
using System;

namespace PlantSafety.Application.Operations.Configuration.Models;

/// <summary>
/// A region-specific override of a feature flag, taking precedence over the global flag
/// (but not over tenant or user overrides) for tenants located in the given region.
/// </summary>
/// <param name="Region">The region this override applies to (e.g. "EU", "US", "APAC").</param>
/// <param name="FeatureName">The feature flag name (matches <see cref="FeatureFlagDto.FlagName"/>).</param>
/// <param name="Enabled">
/// Whether the feature is enabled in this region. When <see langword="false"/>, this is an
/// explicit region-level kill-switch regardless of <see cref="RolloutPercentage"/> or the
/// global flag's state.
/// </param>
/// <param name="RolloutPercentage">
/// Optional region-scoped canary-rollout percentage (0-100), evaluated only when
/// <see cref="Enabled"/> is <see langword="true"/>. NULL means "fully enabled in this
/// region, no percentage gating" once <see cref="Enabled"/> is <see langword="true"/>.
/// </param>
/// <param name="UpdatedUtc">When this override was last changed.</param>
/// <param name="UpdatedBy">Who last changed this override.</param>
public sealed record RegionFeatureFlagOverrideDto(
    string Region,
    string FeatureName,
    bool Enabled,
    int? RolloutPercentage,
    DateTime UpdatedUtc,
    string UpdatedBy);
