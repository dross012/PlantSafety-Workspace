// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operations.Configuration.Resolution
//  Layer:          Application
//  File:           FeatureFlagResolver.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Default IFeatureFlagResolver implementation. Composes the 4 precedence
//                  levels (user > tenant > region > global) using ITenantRolloutPolicy
//                  (Phase 6.6 FeatureRolloutEvaluator) for any level whose Enabled=true but
//                  has a configured, non-null RolloutPercentage.
//
//                  Precedence rule (highest to lowest):
//                    1. User override    -- decides the outcome outright. No percentage
//                       gating at this level: a named user override is a deliberate,
//                       individual on/off (e.g. a beta tester), not a statistical rollout.
//                    2. Tenant override  -- Enabled=false is an explicit kill-switch
//                       (wins outright). Enabled=true with no percentage is a full grant.
//                       Enabled=true with a percentage is evaluated via the rollout bucket
//                       (subject to the tenant's release channel, see below).
//                    3. Region override  -- same structure as tenant override, applied when
//                       no tenant override exists for the current tenant.
//                    4. Global flag      -- same structure again, applied when no more
//                       specific override exists. If no global flag row was found at all,
//                       the result defaults to disabled (FeatureFlagSourceLevel.Default).
//
//                  Canary rollout composition: whichever level's Enabled=true configuration
//                  carries a non-null RolloutPercentage is the one whose percentage gates
//                  inclusion -- i.e. "a canary rollout percentage applies to whichever level
//                  lacks a more specific override" is satisfied automatically by walking the
//                  chain top-down and stopping at the first level that has *any* override
//                  (Enabled+percentage together), consistent with FeatureRolloutEvaluator's
//                  pure, deterministic bucket-by-(tenantId, featureName) contract. When no
//                  tenantId is supplied (anonymous/global evaluation), Guid.Empty is used as
//                  the deterministic bucket subject -- the same "system" sentinel already
//                  used elsewhere in this codebase for non-tenant-scoped rows.
//
//                  Tenant release channel composition (Phase 7 §7.2): at whichever level's
//                  percentage gate applies, a tenant on Beta auto-includes when that level's
//                  RolloutPercentage is non-null and > 0 (an active rollout already started
//                  for the general population); a tenant on Canary auto-includes whenever
//                  RolloutPercentage is non-null at all, including 0 (validating a feature
//                  before its rollout wave begins for anyone). Stable (or no channel
//                  assignment) always falls through to the plain deterministic bucket check.
//                  Additive-only evolution.
// ============================================================================
using System;

using PlantSafety.Application.Common.Rollout;
using PlantSafety.Application.Operations.Configuration.Models;

namespace PlantSafety.Application.Operations.Configuration.Resolution;

/// <summary>
/// Default <see cref="IFeatureFlagResolver"/> implementation, composing the user/tenant/
/// region/global precedence chain with <see cref="ITenantRolloutPolicy"/> canary evaluation
/// and <see cref="TenantReleaseChannel"/> auto-inclusion rules.
/// </summary>
public sealed class FeatureFlagResolver : IFeatureFlagResolver
{
    /// <summary>
    /// Deterministic bucket subject used when no tenant context is supplied, matching the
    /// "system"/non-tenant-scoped sentinel used elsewhere in this codebase.
    /// </summary>
    public static readonly Guid SystemBucketSubject = Guid.Empty;

    private readonly ITenantRolloutPolicy _rolloutPolicy;

    /// <summary>Initialises dependencies.</summary>
    public FeatureFlagResolver(ITenantRolloutPolicy rolloutPolicy)
    {
        _rolloutPolicy = rolloutPolicy ?? throw new ArgumentNullException(nameof(rolloutPolicy));
    }

    /// <inheritdoc/>
    public FeatureFlagResolutionResult Resolve(
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
        TenantReleaseChannel? tenantReleaseChannel)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(featureName);

        // Level 1: User override -- decides outright, no percentage gating.
        if (userOverride is not null)
        {
            return new FeatureFlagResolutionResult(
                featureName, userOverride.Enabled, FeatureFlagSourceLevel.User, null, tenantId, userId, region);
        }

        var channel = tenantReleaseChannel ?? TenantReleaseChannel.Stable;
        var bucketSubject = tenantId ?? SystemBucketSubject;

        // Level 2: Tenant override.
        if (tenantOverride is not null)
        {
            var isEnabled = EvaluateLevel(tenantOverride.Enabled, tenantOverride.RolloutPercentage, featureName, bucketSubject, channel);
            return new FeatureFlagResolutionResult(
                featureName, isEnabled, FeatureFlagSourceLevel.Tenant, tenantOverride.RolloutPercentage, tenantId, userId, region);
        }

        // Level 3: Region override.
        if (regionOverride is not null)
        {
            var isEnabled = EvaluateLevel(regionOverride.Enabled, regionOverride.RolloutPercentage, featureName, bucketSubject, channel);
            return new FeatureFlagResolutionResult(
                featureName, isEnabled, FeatureFlagSourceLevel.Region, regionOverride.RolloutPercentage, tenantId, userId, region);
        }

        // Level 4: Global flag.
        if (globalFlagFound)
        {
            var isEnabled = EvaluateLevel(globalIsEnabled, globalRolloutPercentage, featureName, bucketSubject, channel);
            return new FeatureFlagResolutionResult(
                featureName, isEnabled, FeatureFlagSourceLevel.Global, globalRolloutPercentage, tenantId, userId, region);
        }

        // No override anywhere and no global flag row at all -- safe platform default.
        return new FeatureFlagResolutionResult(
            featureName, false, FeatureFlagSourceLevel.Default, null, tenantId, userId, region);
    }

    /// <summary>
    /// Evaluates a single level's (Enabled, RolloutPercentage) pair into a final boolean,
    /// applying the tenant's release channel to any percentage-gated configuration.
    /// </summary>
    private bool EvaluateLevel(bool enabled, int? rolloutPercentage, string featureName, Guid bucketSubject, TenantReleaseChannel channel)
    {
        if (!enabled)
        {
            // An explicit disable at this level is a hard kill-switch -- no percentage,
            // channel, or lower-level state can override it.
            return false;
        }

        if (rolloutPercentage is null)
        {
            // Enabled with no percentage configured -- a full, ungated grant.
            return true;
        }

        // Enabled with an active percentage-gated (canary) configuration.
        if (channel == TenantReleaseChannel.Canary)
        {
            // Canary tenants validate a feature before its rollout wave begins for anyone
            // else -- any configured percentage (including 0) auto-includes them.
            return true;
        }

        if (channel == TenantReleaseChannel.Beta && rolloutPercentage.Value > 0)
        {
            // Beta tenants auto-include once a rollout has genuinely started for the
            // general population (percentage > 0); a still-dormant 0% rollout does not.
            return true;
        }

        return _rolloutPolicy.IsInRollout(bucketSubject, featureName, rolloutPercentage.Value);
    }
}
