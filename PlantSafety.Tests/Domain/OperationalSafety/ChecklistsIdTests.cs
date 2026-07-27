// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.OperationalSafety.Checklists
//  Layer:          Tests (Unit)
//  File:           ChecklistsIdTests.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Unit tests for ChecklistsId StrongId value object.
// ============================================================================
using PlantSafety.Domain.Operational.Checklists.ValueObjects.Ids;

namespace PlantSafety.Tests.OperationalSafety.Checklists;

public sealed class ChecklistsIdTests : UnitTestBase
{
    [Fact]
    public void New_ShouldReturnNonEmptyId()
    {
        var id = ChecklistsId.New();
        id.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Constructor_WithEmptyGuid_ShouldThrow() =>
        FluentActions.Invoking(() => ChecklistsId.FromGuid(Guid.Empty))
                     .Should().Throw<ArgumentException>();

    [Fact]
    public void TwoIds_WithSameGuid_ShouldBeEqual()
    {
        var g = Guid.NewGuid();
        ChecklistsId.FromGuid(g).Should().Be(ChecklistsId.FromGuid(g));
    }

    [Fact]
    public void TwoIds_DifferentInstances_ShouldNotBeEqual() =>
        ChecklistsId.New().Should().NotBe(ChecklistsId.New());
}
