// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operations.Configuration.Resolution
//  Layer:          Application
//  File:           IFeatureFlagResolver.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Phase 7 (SaaS Productization Blueprint §7.1 Feature Flag System) pure,
//                  deterministic composition of the 4-level feature flag precedence chain:
//                  user > tenant > region > global. Deliberately I/O-free -- callers (see
//                  Queries\ResolveFeatureFlagQueryHandler) fetch the raw override rows and
//                  pass them in, so this contract is fully unit-testable without any
//                  database/network dependency. Additive-only evolution.
// ============================================================================
using System;

using PlantSafety.Application.Operations.Configuration.Models;

namespace PlantSafety.Application.Operations.Configuration.Resolution;

/// <summary>
/// Deterministically composes the user/tenant/region/global feature flag levels into a
/// single resolved outcome. Pure and side-effect-free: the same inputs always produce the
/// same <see cref="FeatureFlagResolutionResult"/>.
/// </summary>
public interface IFeatureFlagResolver
{
    /// <summary>
    /// Resolves the final enabled/disabled outcome for <paramref name="featureName"/> given
    /// the raw state of every precedence level. Levels that don't apply to the current
    /// context (e.g. no tenant override exists, or no <paramref name="tenantId"/> was
    /// supplied) should be passed as <see langword="null"/>.
    /// </summary>
    /// <param name="featureName">The feature flag name being resolved.</param>
    /// <param name="globalIsEnabled">The global flag's <see cref="FeatureFlagDto.IsEnabled"/> value. Used only when no override at any more specific level applies.</param>
    /// <param name="globalRolloutPercentage">The global flag's <see cref="FeatureFlagDto.RolloutPercentage"/> value.</param>
    /// <param name="globalFlagFound">Whether a global flag row was found at all; when <see langword="false"/> and no override applies, the result defaults to disabled (<see cref="FeatureFlagSourceLevel.Default"/>).</param>
    /// <param name="regionOverride">The region-level override for the caller's <paramref name="region"/>, if one exists.</param>
    /// <param name="tenantOverride">The tenant-level override for the caller's <paramref name="tenantId"/>, if one exists.</param>
    /// <param name="userOverride">The user-level override for the caller's <paramref name="userId"/>, if one exists.</param>
    /// <param name="tenantId">The tenant context being evaluated, used as the deterministic rollout-bucket subject; <see langword="null"/> if evaluating outside any tenant context.</param>
    /// <param name="userId">The user context being evaluated, for echoing back in the result.</param>
    /// <param name="region">The region context being evaluated, for echoing back in the result.</param>
    /// <param name="tenantReleaseChannel">The tenant's assigned release channel (defaults to <see cref="TenantReleaseChannel.Stable"/> semantics when <see langword="null"/>), which controls how percentage-gated levels are evaluated for this tenant.</param>
    FeatureFlagResolutionResult Resolve(
        string featureName,
        bool globalIsEnabled,
        int? globalRolloutPercentage,
        bool globalFlagFound,
        RegionFeatureFlagOverrideDto? regionOverride,
        TenantFeatureFlagOverrideDto? tenantOverride,
        UserFeatureFlagOverrideDto? userOverride,
        Guid? tenantId,
        Guid? userId,
        string? region,
        TenantReleaseChannel? tenantReleaseChannel);
}
