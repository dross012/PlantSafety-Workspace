// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Domain.Intelligence.FeatureEngineering
//  Layer:          Tests (Unit)
//  File:           FeatureNameTests.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the FeatureName value object: lowercase/underscore
//                  normalization and the MaxLength guard.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.Intelligence.FeatureEngineering.ValueObjects;

namespace PlantSafety.Tests.Domain.Intelligence.FeatureEngineering;

/// <summary>Unit tests for <see cref="FeatureName"/>.</summary>
public sealed class FeatureNameTests : UnitTestBase
{
    [Theory]
    [InlineData("Worker Fatigue Score", "worker_fatigue_score")]
    [InlineData("ANOMALY SCORE", "anomaly_score")]
    [InlineData("already_normalized", "already_normalized")]
    [InlineData("  padded name  ", "padded_name")]
    public void From_NormalizesToLowercaseSnakeCase(string raw, string expected)
    {
        FeatureName.From(raw).Value.Should().Be(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void From_WithEmptyValue_ThrowsArgumentException(string raw)
    {
        var act = () => FeatureName.From(raw);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void From_ExceedingMaxLength_ThrowsArgumentException()
    {
        var tooLong = new string('a', FeatureName.MaxLength + 1);
        var act = () => FeatureName.From(tooLong);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void From_AtExactlyMaxLength_Succeeds()
    {
        var exact = new string('a', FeatureName.MaxLength);
        FeatureName.From(exact).Value.Should().HaveLength(FeatureName.MaxLength);
    }

    [Fact]
    public void Equality_SameNormalizedValue_AreEqual()
    {
        FeatureName.From("Worker Fatigue").Should().Be(FeatureName.From("worker_fatigue"));
    }

    [Fact]
    public void ToString_ReturnsNormalizedValue()
    {
        FeatureName.From("Anomaly Score").ToString().Should().Be("anomaly_score");
    }
}
