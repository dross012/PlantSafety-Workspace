// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Analytics.Dashboard
//  Layer:          Tests (Unit)
//  File:           DashboardIdTests.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Unit tests for DashboardId StrongId value object.
// ============================================================================
namespace PlantSafety.Tests.Analytics.Dashboard;

public sealed class DashboardIdTests : UnitTestBase
{
    [Fact]
    public void New_ShouldReturnNonEmptyId()
    {
        var id = DashboardId.New();
        id.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Constructor_WithEmptyGuid_ShouldThrow() =>
        FluentActions.Invoking(() => DashboardId.FromGuid(Guid.Empty))
                     .Should().Throw<ArgumentException>();

    [Fact]
    public void TwoIds_DifferentInstances_ShouldNotBeEqual() =>
        DashboardId.New().Should().NotBe(DashboardId.New());
}
