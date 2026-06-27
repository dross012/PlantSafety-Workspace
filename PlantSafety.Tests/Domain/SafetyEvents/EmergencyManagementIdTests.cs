// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.SafetyEvents.EmergencyManagement
//  Layer:          Tests (Unit)
//  File:           EmergencyManagementIdTests.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Unit tests for EmergencyManagementId StrongId value object.
// ============================================================================
namespace PlantSafety.Tests.SafetyEvents.EmergencyManagement;

public sealed class EmergencyManagementIdTests : UnitTestBase
{
    [Fact]
    public void New_ShouldReturnNonEmptyId()
    {
        var id = EmergencyManagementId.New();
        id.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Constructor_WithEmptyGuid_ShouldThrow() =>
        FluentActions.Invoking(() => EmergencyManagementId.FromGuid(Guid.Empty))
                     .Should().Throw<ArgumentException>();

    [Fact]
    public void TwoIds_WithSameGuid_ShouldBeEqual()
    {
        var g = Guid.NewGuid();
        EmergencyManagementId.FromGuid(g).Should().Be(EmergencyManagementId.FromGuid(g));
    }

    [Fact]
    public void TwoIds_DifferentInstances_ShouldNotBeEqual() =>
        EmergencyManagementId.New().Should().NotBe(EmergencyManagementId.New());
}
