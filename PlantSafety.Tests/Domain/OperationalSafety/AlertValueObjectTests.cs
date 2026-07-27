// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.OperationalSafety.Alerts
//  Layer:          Tests (Unit)
//  File:           AlertValueObjectTests.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Tests for AlertSeverity, AlertStatus, and AlertPriority enums/value objects
//                  in the canonical OperationalSafety.Alerts module.
// ============================================================================
namespace PlantSafety.Tests.OperationalSafety.Alerts;

/// <summary>
/// Verifies that <see cref="AlertSeverity"/>, <see cref="AlertStatus"/>,
/// and <see cref="AlertPriority"/> enums are usable as domain discriminators.
/// </summary>
public sealed class AlertValueObjectTests : UnitTestBase
{
    [Fact]
    public void AlertStatus_Created_ShouldBeValidLifecycleState() =>
        AlertStatus.Created.Should().Be(AlertStatus.Created);

    [Fact]
    public void AlertSeverity_All_ShouldContainAllFiveLevels() =>
        AlertSeverity.All.Should().HaveCount(5);

    [Fact]
    public void AlertPriority_All_ShouldContainAllFiveLevels() =>
        AlertPriority.All.Should().HaveCount(5);

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void AlertSeverity_KnownLevel_IsValid(int level) =>
        AlertSeverity.All.Should().Contain(s => s.Level == level && s.IsValid());

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void AlertPriority_KnownLevel_IsValid(int level) =>
        AlertPriority.All.Should().Contain(p => p.Level == level && p.IsValid());
}
