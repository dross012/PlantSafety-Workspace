// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Domain.Intelligence.FeatureEngineering
//  Layer:          Tests (Unit)
//  File:           FeatureVersionTests.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the FeatureVersion value object: MAJOR.MINOR parsing/validation
//                  and the ordinal Supersedes comparison backing
//                  FeatureEngineeringDomainService.Supersedes.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.Intelligence.FeatureEngineering.ValueObjects;

namespace PlantSafety.Tests.Domain.Intelligence.FeatureEngineering;

/// <summary>Unit tests for <see cref="FeatureVersion"/>.</summary>
public sealed class FeatureVersionTests : UnitTestBase
{
    [Theory]
    [InlineData("1.0", 1, 0)]
    [InlineData("2.15", 2, 15)]
    [InlineData("0.1", 0, 1)]
    public void From_WithValidValue_ParsesComponents(string raw, int major, int minor)
    {
        var version = FeatureVersion.From(raw);
        version.Major.Should().Be(major);
        version.Minor.Should().Be(minor);
        version.Value.Should().Be(raw);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("1")]
    [InlineData("1.0.0")]
    [InlineData("v1.0")]
    [InlineData("a.b")]
    public void From_WithInvalidValue_ThrowsArgumentException(string raw)
    {
        var act = () => FeatureVersion.From(raw);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Initial_ReturnsOnePointZero()
    {
        FeatureVersion.Initial.Value.Should().Be("1.0");
    }

    [Theory]
    [InlineData("2.0", "1.0", true)]
    [InlineData("1.1", "1.0", true)]
    [InlineData("1.0", "1.0", false)]
    [InlineData("1.0", "1.1", false)]
    [InlineData("1.0", "2.0", false)]
    [InlineData("1.9", "2.0", false)]
    public void Supersedes_ComparesOrdinally(string left, string right, bool expected)
    {
        var a = FeatureVersion.From(left);
        var b = FeatureVersion.From(right);
        a.Supersedes(b).Should().Be(expected);
    }

    [Fact]
    public void Supersedes_WithNull_ThrowsArgumentNull()
    {
        var version = FeatureVersion.From("1.0");
        var act = () => version.Supersedes(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        FeatureVersion.From("1.2").Should().Be(FeatureVersion.From("1.2"));
    }

    [Fact]
    public void ToString_ReturnsRawValue()
    {
        FeatureVersion.From("1.2").ToString().Should().Be("1.2");
    }
}
