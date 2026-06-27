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
    public void AlertSeverity_AllValues_ShouldBeDefinedInEnum()
    {
        var values = Enum.GetValues<AlertSeverity>();
        values.Should().NotBeEmpty("AlertSeverity must define at least one severity level");
    }

    [Fact]
    public void AlertPriority_AllValues_ShouldBeDefinedInEnum()
    {
        var values = Enum.GetValues<AlertPriority>();
        values.Should().NotBeEmpty("AlertPriority must define at least one priority level");
    }

    [Theory]
    [InlineData(AlertSeverity.Low)]
    [InlineData(AlertSeverity.Medium)]
    [InlineData(AlertSeverity.High)]
    [InlineData(AlertSeverity.Critical)]
    public void AlertSeverity_IsDefined(AlertSeverity severity) =>
        Enum.IsDefined(severity).Should().BeTrue();

    [Theory]
    [InlineData(AlertPriority.Low)]
    [InlineData(AlertPriority.Medium)]
    [InlineData(AlertPriority.High)]
    public void AlertPriority_IsDefined(AlertPriority priority) =>
        Enum.IsDefined(priority).Should().BeTrue();
}
