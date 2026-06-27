// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.SafetyEvents.IncidentManagement
//  Layer:          Tests (Unit)
//  File:           IncidentManagementIdTests.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Unit tests for IncidentManagementId StrongId value object.
// ============================================================================
namespace PlantSafety.Tests.SafetyEvents.IncidentManagement;

public sealed class IncidentManagementIdTests : UnitTestBase
{
    [Fact]
    public void New_ShouldReturnNonEmptyId()
    {
        var id = IncidentManagementId.New();
        id.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Constructor_WithEmptyGuid_ShouldThrow() =>
        FluentActions.Invoking(() => IncidentManagementId.FromGuid(Guid.Empty))
                     .Should().Throw<ArgumentException>();

    [Fact]
    public void TwoIds_WithSameGuid_ShouldBeEqual()
    {
        var g = Guid.NewGuid();
        IncidentManagementId.FromGuid(g).Should().Be(IncidentManagementId.FromGuid(g));
    }

    [Fact]
    public void TwoIds_DifferentInstances_ShouldNotBeEqual() =>
        IncidentManagementId.New().Should().NotBe(IncidentManagementId.New());
}
