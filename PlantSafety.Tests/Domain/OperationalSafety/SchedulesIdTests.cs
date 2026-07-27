// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.OperationalSafety.Schedules
//  Layer:          Tests (Unit)
//  File:           SchedulesIdTests.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Unit tests for SchedulesId StrongId value object.
// ============================================================================
using PlantSafety.Domain.Operational.Schedules.ValueObjects.Ids;

namespace PlantSafety.Tests.OperationalSafety.Schedules;

public sealed class SchedulesIdTests : UnitTestBase
{
    [Fact]
    public void New_ShouldReturnNonEmptyId()
    {
        var id = SchedulesId.New();
        id.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Constructor_WithEmptyGuid_ShouldThrow() =>
        FluentActions.Invoking(() => SchedulesId.FromGuid(Guid.Empty))
                     .Should().Throw<ArgumentException>();

    [Fact]
    public void TwoIds_WithSameGuid_ShouldBeEqual()
    {
        var g = Guid.NewGuid();
        SchedulesId.FromGuid(g).Should().Be(SchedulesId.FromGuid(g));
    }

    [Fact]
    public void TwoIds_DifferentInstances_ShouldNotBeEqual() =>
        SchedulesId.New().Should().NotBe(SchedulesId.New());
}
