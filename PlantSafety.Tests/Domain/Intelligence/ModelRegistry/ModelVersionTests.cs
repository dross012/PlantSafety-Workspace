// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Domain.Intelligence.ModelRegistry
//  Layer:          Tests (Unit)
//  File:           ModelVersionTests.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the ModelVersion value object: SemVer parsing/validation and
//                  the ordinal IsGreaterThan comparison backing
//                  ModelRegistryDomainService.SupersedesCurrentDeployment.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.Intelligence.ModelRegistry.ValueObjects;

namespace PlantSafety.Tests.Domain.Intelligence.ModelRegistry;

/// <summary>Unit tests for <see cref="ModelVersion"/>.</summary>
public sealed class ModelVersionTests : UnitTestBase
{
    [Theory]
    [InlineData("1.0.0", 1, 0, 0)]
    [InlineData("2.15.3", 2, 15, 3)]
    [InlineData("0.0.1", 0, 0, 1)]
    public void From_WithValidSemVer_ParsesComponents(string raw, int major, int minor, int patch)
    {
        var version = ModelVersion.From(raw);
        version.Major.Should().Be(major);
        version.Minor.Should().Be(minor);
        version.Patch.Should().Be(patch);
        version.Value.Should().Be(raw);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("1.0")]
    [InlineData("1.0.0.0")]
    [InlineData("v1.0.0")]
    [InlineData("a.b.c")]
    public void From_WithInvalidValue_ThrowsArgumentException(string raw)
    {
        var act = () => ModelVersion.From(raw);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void From_TrimsWhitespace()
    {
        var version = ModelVersion.From("  1.2.3  ");
        version.Value.Should().Be("1.2.3");
    }

    [Fact]
    public void TryParse_WithValidValue_ReturnsTrueAndResult()
    {
        var success = ModelVersion.TryParse("1.2.3", out var result);
        success.Should().BeTrue();
        result.Should().NotBeNull();
        result!.Value.Should().Be("1.2.3");
    }

    [Fact]
    public void TryParse_WithInvalidValue_ReturnsFalseAndNullResult()
    {
        var success = ModelVersion.TryParse("not-a-version", out var result);
        success.Should().BeFalse();
        result.Should().BeNull();
    }

    [Theory]
    [InlineData("2.0.0", "1.0.0", true)]
    [InlineData("1.1.0", "1.0.0", true)]
    [InlineData("1.0.1", "1.0.0", true)]
    [InlineData("1.0.0", "1.0.0", false)]
    [InlineData("1.0.0", "1.0.1", false)]
    [InlineData("1.0.0", "1.1.0", false)]
    [InlineData("1.0.0", "2.0.0", false)]
    [InlineData("1.9.9", "2.0.0", false)]
    public void IsGreaterThan_ComparesOrdinally(string left, string right, bool expected)
    {
        var a = ModelVersion.From(left);
        var b = ModelVersion.From(right);
        a.IsGreaterThan(b).Should().Be(expected);
    }

    [Fact]
    public void IsGreaterThan_WithNull_ThrowsArgumentNull()
    {
        var version = ModelVersion.From("1.0.0");
        var act = () => version.IsGreaterThan(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var a = ModelVersion.From("1.2.3");
        var b = ModelVersion.From("1.2.3");
        a.Should().Be(b);
    }

    [Fact]
    public void ToString_ReturnsRawValue()
    {
        ModelVersion.From("1.2.3").ToString().Should().Be("1.2.3");
    }
}
