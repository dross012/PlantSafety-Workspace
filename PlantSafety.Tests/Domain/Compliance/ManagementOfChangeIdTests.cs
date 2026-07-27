// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Compliance.ManagementOfChange
//  Layer:          Tests (Unit)
//  File:           ManagementOfChangeIdTests.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Unit tests for ManagementOfChangeId StrongId value object.
// ============================================================================
using PlantSafety.Domain.Compliance.ManagementOfChange.ValueObjects.Ids;

namespace PlantSafety.Tests.Compliance.ManagementOfChange;

public sealed class ManagementOfChangeIdTests : UnitTestBase
{
    [Fact]
    public void New_ShouldReturnNonEmptyId()
    {
        var id = ManagementOfChangeId.New();
        id.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Constructor_WithEmptyGuid_ShouldThrow() =>
        FluentActions.Invoking(() => ManagementOfChangeId.FromGuid(Guid.Empty))
                     .Should().Throw<ArgumentException>();

    [Fact]
    public void TwoIds_WithSameGuid_ShouldBeEqual()
    {
        var g = Guid.NewGuid();
        ManagementOfChangeId.FromGuid(g).Should().Be(ManagementOfChangeId.FromGuid(g));
    }

    [Fact]
    public void TwoIds_WithDifferentGuids_ShouldNotBeEqual() =>
        ManagementOfChangeId.New().Should().NotBe(ManagementOfChangeId.New());
}
