// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Domain.Analytics.PredictiveAnalytics
//  Layer:          Tests (Unit)
//  File:           PredictionConfidenceTests.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the PredictionConfidence value object: clamping, qualitative
//                  banding boundaries, and the FromSampleCount saturating curve.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.Analytics.PredictiveAnalytics.Predictors;

namespace PlantSafety.Tests.Domain.Analytics.PredictiveAnalytics;

/// <summary>Unit tests for <see cref="PredictionConfidence"/>.</summary>
public sealed class PredictionConfidenceTests : UnitTestBase
{
    [Theory]
    [InlineData(-1.0, 0.0)]
    [InlineData(0.0, 0.0)]
    [InlineData(0.5, 0.5)]
    [InlineData(1.0, 1.0)]
    [InlineData(2.0, 1.0)]
    public void Constructor_ShouldClampToUnitInterval(double raw, double expected)
    {
        new PredictionConfidence(raw).Value.Should().Be(expected);
    }

    [Theory]
    [InlineData(0.0, PredictionConfidenceBand.Low)]
    [InlineData(0.39, PredictionConfidenceBand.Low)]
    [InlineData(0.40, PredictionConfidenceBand.Medium)]
    [InlineData(0.74, PredictionConfidenceBand.Medium)]
    [InlineData(0.75, PredictionConfidenceBand.High)]
    [InlineData(1.0, PredictionConfidenceBand.High)]
    public void Band_ShouldReflectValueBoundaries(double value, PredictionConfidenceBand expected)
    {
        new PredictionConfidence(value).Band.Should().Be(expected);
    }

    [Fact]
    public void FromSampleCount_WithZeroSamples_ShouldBeZero()
    {
        PredictionConfidence.FromSampleCount(0).Value.Should().Be(0.0);
    }

    [Fact]
    public void FromSampleCount_WithNegativeSamples_ShouldClampToZero()
    {
        PredictionConfidence.FromSampleCount(-5).Value.Should().Be(0.0);
    }

    [Fact]
    public void FromSampleCount_AtFullConfidenceThreshold_ShouldBeOne()
    {
        PredictionConfidence.FromSampleCount(30, samplesForFullConfidence: 30).Value.Should().Be(1.0);
    }

    [Fact]
    public void FromSampleCount_AboveFullConfidenceThreshold_ShouldClampToOne()
    {
        PredictionConfidence.FromSampleCount(300, samplesForFullConfidence: 30).Value.Should().Be(1.0);
    }

    [Fact]
    public void FromSampleCount_HalfwayToThreshold_ShouldBeHalf()
    {
        PredictionConfidence.FromSampleCount(15, samplesForFullConfidence: 30).Value.Should().Be(0.5);
    }

    [Fact]
    public void FromSampleCount_WithNonPositiveFullConfidenceSamples_ShouldThrow()
    {
        var act = () => PredictionConfidence.FromSampleCount(1, samplesForFullConfidence: 0);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void ToString_ShouldIncludePercentAndBand()
    {
        new PredictionConfidence(0.8).ToString().Should().Contain("80").And.Contain("High");
    }
}
