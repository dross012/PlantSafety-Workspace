// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Application.Operations.Configuration.Resolution
//  Layer:          Tests (Unit)
//  File:           FeatureFlagResolverTests.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Unit tests for FeatureFlagResolver (Phase 7 §7.1). Covers every
//                  precedence combination of the 4-level chain (user > tenant > region >
//                  global), percentage-gated canary evaluation, and TenantReleaseChannel
//                  auto-inclusion rules. Uses a substituted ITenantRolloutPolicy so bucket
//                  membership is fully deterministic/controllable per test.
// ============================================================================
using System;

using NSubstitute;

using PlantSafety.Application.Common.Rollout;
using PlantSafety.Application.Operations.Configuration.Models;
using PlantSafety.Application.Operations.Configuration.Resolution;

using Xunit;

namespace PlantSafety.Tests.Application.Operations.Configuration.Resolution;

/// <summary>Tests for <see cref="FeatureFlagResolver"/>.</summary>
public sealed class FeatureFlagResolverTests
{
    private const string Feature = "Intelligence.NewDashboard";

    private readonly ITenantRolloutPolicy _rolloutPolicy = Substitute.For<ITenantRolloutPolicy>();
    private FeatureFlagResolver Sut() => new(_rolloutPolicy);

    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();
    private const string Region = "EU";

    // ---------------------------------------------------------------
    // Level 1: User override -- highest precedence, decides outright.
    // ---------------------------------------------------------------

    [Fact]
    public void Resolve_UserOverrideEnabled_WinsOverEverythingElseDisabled()
    {
        var sut = Sut();
        var userOverride = new UserFeatureFlagOverrideDto(UserId, Feature, true, DateTime.UtcNow, "admin");
        var tenantOverride = new TenantFeatureFlagOverrideDto(TenantId, Feature, false, null, DateTime.UtcNow, "admin");
        var regionOverride = new RegionFeatureFlagOverrideDto(Region, Feature, false, null, DateTime.UtcNow, "admin");

        var result = sut.Resolve(Feature, globalIsEnabled: false, globalRolloutPercentage: null, globalFlagFound: true,
            regionOverride, tenantOverride, userOverride, TenantId, UserId, Region, TenantReleaseChannel.Stable);

        Assert.True(result.IsEnabled);
        Assert.Equal(FeatureFlagSourceLevel.User, result.DecidedBy);
    }

    [Fact]
    public void Resolve_UserOverrideDisabled_WinsOverEverythingElseEnabled()
    {
        var sut = Sut();
        var userOverride = new UserFeatureFlagOverrideDto(UserId, Feature, false, DateTime.UtcNow, "admin");
        var tenantOverride = new TenantFeatureFlagOverrideDto(TenantId, Feature, true, null, DateTime.UtcNow, "admin");

        var result = sut.Resolve(Feature, globalIsEnabled: true, globalRolloutPercentage: null, globalFlagFound: true,
            null, tenantOverride, userOverride, TenantId, UserId, Region, TenantReleaseChannel.Stable);

        Assert.False(result.IsEnabled);
        Assert.Equal(FeatureFlagSourceLevel.User, result.DecidedBy);
    }

    [Fact]
    public void Resolve_UserOverride_NeverConsultsRolloutPolicy()
    {
        var sut = Sut();
        var userOverride = new UserFeatureFlagOverrideDto(UserId, Feature, true, DateTime.UtcNow, "admin");

        sut.Resolve(Feature, true, 50, true, null, null, userOverride, TenantId, UserId, Region, TenantReleaseChannel.Stable);

        _rolloutPolicy.DidNotReceiveWithAnyArgs().IsInRollout(default, default!, default);
    }

    // ---------------------------------------------------------------
    // Level 2: Tenant override.
    // ---------------------------------------------------------------

    [Fact]
    public void Resolve_TenantOverrideDisabled_IsHardKillSwitch_RegardlessOfPercentage()
    {
        var sut = Sut();
        var tenantOverride = new TenantFeatureFlagOverrideDto(TenantId, Feature, false, 100, DateTime.UtcNow, "admin");
        _rolloutPolicy.IsInRollout(TenantId, Feature, Arg.Any<int>()).Returns(true);

        var result = sut.Resolve(Feature, true, 100, true, null, tenantOverride, null, TenantId, null, null, TenantReleaseChannel.Stable);

        Assert.False(result.IsEnabled);
        Assert.Equal(FeatureFlagSourceLevel.Tenant, result.DecidedBy);
    }

    [Fact]
    public void Resolve_TenantOverrideEnabledNoPercentage_IsFullGrant()
    {
        var sut = Sut();
        var tenantOverride = new TenantFeatureFlagOverrideDto(TenantId, Feature, true, null, DateTime.UtcNow, "admin");

        var result = sut.Resolve(Feature, false, null, true, null, tenantOverride, null, TenantId, null, null, TenantReleaseChannel.Stable);

        Assert.True(result.IsEnabled);
        Assert.Equal(FeatureFlagSourceLevel.Tenant, result.DecidedBy);
        _rolloutPolicy.DidNotReceiveWithAnyArgs().IsInRollout(default, default!, default);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public void Resolve_TenantOverrideEnabledWithPercentage_DelegatesToRolloutPolicy(bool bucketResult, bool expectedEnabled)
    {
        var sut = Sut();
        var tenantOverride = new TenantFeatureFlagOverrideDto(TenantId, Feature, true, 40, DateTime.UtcNow, "admin");
        _rolloutPolicy.IsInRollout(TenantId, Feature, 40).Returns(bucketResult);

        var result = sut.Resolve(Feature, false, null, true, null, tenantOverride, null, TenantId, null, null, TenantReleaseChannel.Stable);

        Assert.Equal(expectedEnabled, result.IsEnabled);
        Assert.Equal(FeatureFlagSourceLevel.Tenant, result.DecidedBy);
        _rolloutPolicy.Received(1).IsInRollout(TenantId, Feature, 40);
    }

    [Fact]
    public void Resolve_TenantOverride_WinsOverRegionAndGlobal()
    {
        var sut = Sut();
        var tenantOverride = new TenantFeatureFlagOverrideDto(TenantId, Feature, true, null, DateTime.UtcNow, "admin");
        var regionOverride = new RegionFeatureFlagOverrideDto(Region, Feature, false, null, DateTime.UtcNow, "admin");

        var result = sut.Resolve(Feature, false, null, true, regionOverride, tenantOverride, null, TenantId, null, Region, TenantReleaseChannel.Stable);

        Assert.True(result.IsEnabled);
        Assert.Equal(FeatureFlagSourceLevel.Tenant, result.DecidedBy);
    }

    // ---------------------------------------------------------------
    // Level 3: Region override -- applies only when no tenant override.
    // ---------------------------------------------------------------

    [Fact]
    public void Resolve_RegionOverrideDisabled_IsHardKillSwitch()
    {
        var sut = Sut();
        var regionOverride = new RegionFeatureFlagOverrideDto(Region, Feature, false, null, DateTime.UtcNow, "admin");

        var result = sut.Resolve(Feature, true, null, true, regionOverride, null, null, TenantId, null, Region, TenantReleaseChannel.Stable);

        Assert.False(result.IsEnabled);
        Assert.Equal(FeatureFlagSourceLevel.Region, result.DecidedBy);
    }

    [Fact]
    public void Resolve_RegionOverrideEnabledNoPercentage_IsFullGrant()
    {
        var sut = Sut();
        var regionOverride = new RegionFeatureFlagOverrideDto(Region, Feature, true, null, DateTime.UtcNow, "admin");

        var result = sut.Resolve(Feature, false, null, true, regionOverride, null, null, TenantId, null, Region, TenantReleaseChannel.Stable);

        Assert.True(result.IsEnabled);
        Assert.Equal(FeatureFlagSourceLevel.Region, result.DecidedBy);
    }

    [Fact]
    public void Resolve_RegionOverrideEnabledWithPercentage_DelegatesToRolloutPolicy()
    {
        var sut = Sut();
        var regionOverride = new RegionFeatureFlagOverrideDto(Region, Feature, true, 25, DateTime.UtcNow, "admin");
        _rolloutPolicy.IsInRollout(TenantId, Feature, 25).Returns(true);

        var result = sut.Resolve(Feature, false, null, true, regionOverride, null, null, TenantId, null, Region, TenantReleaseChannel.Stable);

        Assert.True(result.IsEnabled);
        Assert.Equal(FeatureFlagSourceLevel.Region, result.DecidedBy);
    }

    [Fact]
    public void Resolve_RegionOverride_WinsOverGlobal_WhenNoTenantOverride()
    {
        var sut = Sut();
        var regionOverride = new RegionFeatureFlagOverrideDto(Region, Feature, true, null, DateTime.UtcNow, "admin");

        var result = sut.Resolve(Feature, false, null, true, regionOverride, null, null, TenantId, null, Region, TenantReleaseChannel.Stable);

        Assert.True(result.IsEnabled);
        Assert.Equal(FeatureFlagSourceLevel.Region, result.DecidedBy);
    }

    // ---------------------------------------------------------------
    // Level 4: Global flag -- applies only when no more specific override.
    // ---------------------------------------------------------------

    [Fact]
    public void Resolve_GlobalFlagDisabled_NoOverridesAnywhere_ResultDisabled()
    {
        var sut = Sut();

        var result = sut.Resolve(Feature, false, null, true, null, null, null, TenantId, null, null, TenantReleaseChannel.Stable);

        Assert.False(result.IsEnabled);
        Assert.Equal(FeatureFlagSourceLevel.Global, result.DecidedBy);
    }

    [Fact]
    public void Resolve_GlobalFlagEnabledNoPercentage_NoOverridesAnywhere_ResultEnabled()
    {
        var sut = Sut();

        var result = sut.Resolve(Feature, true, null, true, null, null, null, TenantId, null, null, TenantReleaseChannel.Stable);

        Assert.True(result.IsEnabled);
        Assert.Equal(FeatureFlagSourceLevel.Global, result.DecidedBy);
    }

    [Fact]
    public void Resolve_GlobalFlagEnabledWithPercentage_DelegatesToRolloutPolicy()
    {
        var sut = Sut();
        _rolloutPolicy.IsInRollout(TenantId, Feature, 10).Returns(false);

        var result = sut.Resolve(Feature, true, 10, true, null, null, null, TenantId, null, null, TenantReleaseChannel.Stable);

        Assert.False(result.IsEnabled);
        Assert.Equal(FeatureFlagSourceLevel.Global, result.DecidedBy);
    }

    [Fact]
    public void Resolve_NoGlobalFlagFound_NoOverridesAnywhere_DefaultsToDisabled()
    {
        var sut = Sut();

        var result = sut.Resolve(Feature, false, null, globalFlagFound: false, null, null, null, TenantId, null, null, TenantReleaseChannel.Stable);

        Assert.False(result.IsEnabled);
        Assert.Equal(FeatureFlagSourceLevel.Default, result.DecidedBy);
    }

    // ---------------------------------------------------------------
    // Bucket subject fallback: Guid.Empty used when no tenant supplied.
    // ---------------------------------------------------------------

    [Fact]
    public void Resolve_NoTenantIdSupplied_UsesSystemSentinelAsBucketSubject()
    {
        var sut = Sut();
        _rolloutPolicy.IsInRollout(Guid.Empty, Feature, 50).Returns(true);

        var result = sut.Resolve(Feature, true, 50, true, null, null, null, tenantId: null, null, null, TenantReleaseChannel.Stable);

        Assert.True(result.IsEnabled);
        _rolloutPolicy.Received(1).IsInRollout(Guid.Empty, Feature, 50);
    }

    // ---------------------------------------------------------------
    // TenantReleaseChannel auto-inclusion rules.
    // ---------------------------------------------------------------

    [Fact]
    public void Resolve_BetaChannel_AutoIncludes_WhenPercentageGreaterThanZero()
    {
        var sut = Sut();
        _rolloutPolicy.IsInRollout(Arg.Any<Guid>(), Feature, Arg.Any<int>()).Returns(false); // bucket would say NO

        var result = sut.Resolve(Feature, true, 5, true, null, null, null, TenantId, null, null, TenantReleaseChannel.Beta);

        Assert.True(result.IsEnabled);
        _rolloutPolicy.DidNotReceiveWithAnyArgs().IsInRollout(default, default!, default);
    }

    [Fact]
    public void Resolve_BetaChannel_DoesNotAutoInclude_WhenPercentageIsZero()
    {
        var sut = Sut();
        _rolloutPolicy.IsInRollout(TenantId, Feature, 0).Returns(false);

        var result = sut.Resolve(Feature, true, 0, true, null, null, null, TenantId, null, null, TenantReleaseChannel.Beta);

        Assert.False(result.IsEnabled);
        _rolloutPolicy.Received(1).IsInRollout(TenantId, Feature, 0);
    }

    [Fact]
    public void Resolve_CanaryChannel_AutoIncludes_EvenWhenPercentageIsZero()
    {
        var sut = Sut();
        _rolloutPolicy.IsInRollout(Arg.Any<Guid>(), Feature, Arg.Any<int>()).Returns(false); // bucket would say NO

        var result = sut.Resolve(Feature, true, 0, true, null, null, null, TenantId, null, null, TenantReleaseChannel.Canary);

        Assert.True(result.IsEnabled);
        _rolloutPolicy.DidNotReceiveWithAnyArgs().IsInRollout(default, default!, default);
    }

    [Fact]
    public void Resolve_CanaryChannel_DoesNotBypass_ExplicitDisable()
    {
        var sut = Sut();
        var tenantOverride = new TenantFeatureFlagOverrideDto(TenantId, Feature, false, 50, DateTime.UtcNow, "admin");

        var result = sut.Resolve(Feature, true, 50, true, null, tenantOverride, null, TenantId, null, null, TenantReleaseChannel.Canary);

        Assert.False(result.IsEnabled);
        Assert.Equal(FeatureFlagSourceLevel.Tenant, result.DecidedBy);
    }

    [Fact]
    public void Resolve_StableChannel_AlwaysUsesDeterministicBucket_EvenAtLowPercentage()
    {
        var sut = Sut();
        _rolloutPolicy.IsInRollout(TenantId, Feature, 1).Returns(false);

        var result = sut.Resolve(Feature, true, 1, true, null, null, null, TenantId, null, null, TenantReleaseChannel.Stable);

        Assert.False(result.IsEnabled);
        _rolloutPolicy.Received(1).IsInRollout(TenantId, Feature, 1);
    }

    [Fact]
    public void Resolve_NullChannel_BehavesLikeStable()
    {
        var sut = Sut();
        _rolloutPolicy.IsInRollout(TenantId, Feature, 30).Returns(true);

        var result = sut.Resolve(Feature, true, 30, true, null, null, null, TenantId, null, null, tenantReleaseChannel: null);

        Assert.True(result.IsEnabled);
        _rolloutPolicy.Received(1).IsInRollout(TenantId, Feature, 30);
    }

    // ---------------------------------------------------------------
    // Guard clauses / result echo-back.
    // ---------------------------------------------------------------

    [Fact]
    public void Resolve_EmptyFeatureName_Throws()
    {
        var sut = Sut();

        Assert.Throws<ArgumentException>(() =>
            sut.Resolve(string.Empty, true, null, true, null, null, null, TenantId, UserId, Region, TenantReleaseChannel.Stable));
    }

    [Fact]
    public void Resolve_EchoesBackContext()
    {
        var sut = Sut();

        var result = sut.Resolve(Feature, true, null, true, null, null, null, TenantId, UserId, Region, TenantReleaseChannel.Stable);

        Assert.Equal(Feature, result.FeatureName);
        Assert.Equal(TenantId, result.TenantId);
        Assert.Equal(UserId, result.UserId);
        Assert.Equal(Region, result.Region);
    }
}
