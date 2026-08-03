// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Unit tests for FeatureRolloutEvaluator (Phase 6.6, §6.2), verifying
//   determinism, boundary behavior at 0/100, and monotonic growth of the rollout wave as
//   the configured percentage increases.
// SchemaVersion: 1

using PlantSafety.Application.Common.Rollout;
using Xunit;

namespace PlantSafety.Tests.Application.Common.Rollout;

/// <summary>Tests for <see cref="FeatureRolloutEvaluator"/>.</summary>
public sealed class FeatureRolloutEvaluatorTests
{
    private readonly FeatureRolloutEvaluator _sut = new();

    [Fact]
    public void IsInRollout_SameInputs_AlwaysReturnsSameResult()
    {
        var tenantId = Guid.NewGuid();

        var first = _sut.IsInRollout(tenantId, "Intelligence.NewDashboard", 42);
        var second = _sut.IsInRollout(tenantId, "Intelligence.NewDashboard", 42);
        var third = _sut.IsInRollout(tenantId, "Intelligence.NewDashboard", 42);

        Assert.Equal(first, second);
        Assert.Equal(second, third);
    }

    [Fact]
    public void IsInRollout_ZeroPercent_AlwaysFalse()
    {
        for (var i = 0; i < 20; i++)
        {
            Assert.False(_sut.IsInRollout(Guid.NewGuid(), "AnyFeature", 0));
        }
    }

    [Fact]
    public void IsInRollout_HundredPercent_AlwaysTrue()
    {
        for (var i = 0; i < 20; i++)
        {
            Assert.True(_sut.IsInRollout(Guid.NewGuid(), "AnyFeature", 100));
        }
    }

    [Fact]
    public void IsInRollout_IncreasingPercentage_IsMonotonic()
    {
        // A tenant included at percentage P must remain included at every percentage > P --
        // the rollout wave only ever grows, never reshuffles, as the percentage increases.
        var tenantId = Guid.NewGuid();
        const string featureName = "Diagnostics.CanaryFeature";

        var firstIncludedAt = -1;
        for (var percentage = 1; percentage <= 100; percentage++)
        {
            if (_sut.IsInRollout(tenantId, featureName, percentage))
            {
                firstIncludedAt = percentage;
                break;
            }
        }

        Assert.True(firstIncludedAt > 0, "Tenant should be included by percentage 100 at the latest.");

        for (var percentage = firstIncludedAt; percentage <= 100; percentage++)
        {
            Assert.True(_sut.IsInRollout(tenantId, featureName, percentage));
        }
    }

    [Fact]
    public void IsInRollout_DifferentFeatureNames_CanDifferForSameTenant()
    {
        // Not a hard guarantee for any specific pair, but across many feature names for one
        // tenant at 50%, we expect a reasonably even split -- guards against a degenerate
        // hash implementation that ignores featureName entirely.
        var tenantId = Guid.NewGuid();
        var includedCount = 0;

        for (var i = 0; i < 200; i++)
        {
            if (_sut.IsInRollout(tenantId, $"Feature.{i}", 50))
            {
                includedCount++;
            }
        }

        Assert.InRange(includedCount, 60, 140);
    }

    [Fact]
    public void IsInRollout_EmptyFeatureName_Throws()
    {
        Assert.Throws<ArgumentException>(() => _sut.IsInRollout(Guid.NewGuid(), string.Empty, 50));
    }
}
