// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operations.Configuration
//  Layer:          Application
//  File:           FeatureFlagResolutionResult.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Phase 7 (§7.1) output of the 4-level feature flag precedence resolver
//                  (Resolution\FeatureFlagResolver). Additive-only evolution.
// ============================================================================
using System;

namespace PlantSafety.Application.Operations.Configuration.Models;

/// <summary>The deterministic outcome of resolving a feature flag across all 4 precedence levels.</summary>
/// <param name="FeatureName">The feature flag name that was resolved.</param>
/// <param name="IsEnabled">The final, resolved on/off decision.</param>
/// <param name="DecidedBy">Which precedence level decided <see cref="IsEnabled"/>.</param>
/// <param name="EffectiveRolloutPercentage">
/// The rollout percentage of the level that decided the outcome, if any was configured at
/// that level (informational only -- <see cref="IsEnabled"/> already reflects its evaluation).
/// </param>
/// <param name="TenantId">The tenant context the resolution was evaluated for, if any.</param>
/// <param name="UserId">The user context the resolution was evaluated for, if any.</param>
/// <param name="Region">The region context the resolution was evaluated for, if any.</param>
public sealed record FeatureFlagResolutionResult(
    string FeatureName,
    bool IsEnabled,
    FeatureFlagSourceLevel DecidedBy,
    int? EffectiveRolloutPercentage,
    Guid? TenantId,
    Guid? UserId,
    string? Region);
