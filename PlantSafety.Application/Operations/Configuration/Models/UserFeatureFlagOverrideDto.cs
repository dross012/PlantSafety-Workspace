// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operations.Configuration
//  Layer:          Application
//  File:           UserFeatureFlagOverrideDto.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Phase 7 (SaaS Productization Blueprint §7.1) user-level feature flag
//                  override -- e.g. an individual beta tester. This is the highest-precedence
//                  level in the resolution chain (user > tenant > region > global): when
//                  present, it decides the final outcome outright, with no percentage-based
//                  canary gating (an individual user override is a direct, deliberate on/off,
//                  not a statistical rollout wave). See Resolution\FeatureFlagResolver.
//                  Additive-only evolution.
// ============================================================================
using System;

namespace PlantSafety.Application.Operations.Configuration.Models;

/// <summary>
/// An individual user-specific override of a feature flag -- the highest-precedence level
/// in the 4-level resolution chain (user > tenant > region > global).
/// </summary>
/// <param name="UserId">The user this override applies to.</param>
/// <param name="FeatureName">The feature flag name (matches <see cref="FeatureFlagDto.FlagName"/>).</param>
/// <param name="Enabled">Whether the feature is enabled for this specific user. Decides the outcome outright -- no percentage-based gating at this level.</param>
/// <param name="UpdatedUtc">When this override was last changed.</param>
/// <param name="UpdatedBy">Who last changed this override.</param>
public sealed record UserFeatureFlagOverrideDto(
    Guid UserId,
    string FeatureName,
    bool Enabled,
    DateTime UpdatedUtc,
    string UpdatedBy);
