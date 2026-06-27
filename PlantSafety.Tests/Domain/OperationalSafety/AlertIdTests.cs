// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Domain.OperationalSafety
//  Layer:          Tests (Unit)
//  File:           AlertIdTests.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the AlertId strong-typed identifier.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.Operational.Alerts.ValueObjects.Ids;

namespace PlantSafety.Tests.Domain.OperationalSafety;

/// <summary>
/// Unit tests for <see cref="AlertId"/> — verifies construction invariants,
/// empty-GUID rejection, equality, parsing, and string representation.
/// </summary>
public sealed class AlertIdTests
{
    [Fact]
    public void New_ShouldReturnNonEmptyId()
    {
        var id = AlertId.New();
        id.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Constructor_WithEmptyGuid_ShouldThrow()
    {
        var act = () => new AlertId(Guid.Empty);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void FromGuid_WithValidGuid_ShouldPreserveValue()
    {
        var guid = Guid.NewGuid();
        var id = AlertId.FromGuid(guid);
        id.Value.Should().Be(guid);
    }

    [Fact]
    public void Parse_WithValidString_ShouldReturnCorrectId()
    {
        var guid = Guid.NewGuid();
        var id = AlertId.Parse(guid.ToString("D"));
        id.Value.Should().Be(guid);
    }

    [Fact]
    public void TwoIds_WithSameGuid_ShouldBeEqual()
    {
        var guid = Guid.NewGuid();
        var a = AlertId.FromGuid(guid);
        var b = AlertId.FromGuid(guid);
        a.Should().Be(b);
    }

    [Fact]
    public void TwoIds_WithDifferentGuids_ShouldNotBeEqual()
    {
        AlertId.New().Should().NotBe(AlertId.New());
    }

    [Fact]
    public void ToString_ShouldReturnGuidDFormat()
    {
        var guid = Guid.NewGuid();
        var id = AlertId.FromGuid(guid);
        id.ToString().Should().Be(guid.ToString("D"));
    }
}
