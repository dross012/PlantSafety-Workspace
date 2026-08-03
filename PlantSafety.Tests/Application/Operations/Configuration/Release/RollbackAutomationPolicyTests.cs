// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Application.Operations.Configuration.Release
//  Layer:          Tests (Unit)
//  File:           RollbackAutomationPolicyTests.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Unit tests for RollbackAutomationPolicy (Phase 7 §7.2).
// ============================================================================
using PlantSafety.Application.Operations.Configuration.Release;

using Xunit;

namespace PlantSafety.Tests.Application.Operations.Configuration.Release;

/// <summary>Tests for <see cref="RollbackAutomationPolicy"/>.</summary>
public sealed class RollbackAutomationPolicyTests
{
    private readonly RollbackAutomationPolicy _sut = new();

    [Fact]
    public void ShouldRollback_ErrorRateExceedsThreshold_EnoughSamples_ReturnsTrue()
    {
        Assert.True(_sut.ShouldRollback(observedErrorRate: 0.08, errorRateThreshold: 0.02, observedRequestCount: 500, minimumSampleSize: 100));
    }

    [Fact]
    public void ShouldRollback_ErrorRateBelowThreshold_ReturnsFalse()
    {
        Assert.False(_sut.ShouldRollback(observedErrorRate: 0.01, errorRateThreshold: 0.02, observedRequestCount: 500, minimumSampleSize: 100));
    }

    [Fact]
    public void ShouldRollback_ErrorRateExceedsThreshold_ButNotEnoughSamples_ReturnsFalse()
    {
        // 1 failed request out of 1 total would otherwise look like a 100% error rate.
        Assert.False(_sut.ShouldRollback(observedErrorRate: 1.0, errorRateThreshold: 0.02, observedRequestCount: 1, minimumSampleSize: 100));
    }

    [Fact]
    public void ShouldRollback_ErrorRateExactlyAtThreshold_ReturnsFalse()
    {
        // Strictly greater-than, not greater-or-equal -- exactly at threshold is tolerated.
        Assert.False(_sut.ShouldRollback(observedErrorRate: 0.02, errorRateThreshold: 0.02, observedRequestCount: 1000, minimumSampleSize: 100));
    }

    [Fact]
    public void ShouldRollback_SampleSizeExactlyAtMinimum_IsTrusted()
    {
        Assert.True(_sut.ShouldRollback(observedErrorRate: 0.5, errorRateThreshold: 0.02, observedRequestCount: 100, minimumSampleSize: 100));
    }
}
