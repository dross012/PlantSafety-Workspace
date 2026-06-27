// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Compliance.Permits
//  Layer:          Tests (Unit)
//  File:           PermitsIdTests.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Unit tests for PermitsId StrongId value object.
// ============================================================================
namespace PlantSafety.Tests.Compliance.Permits;

public sealed class PermitsIdTests : UnitTestBase
{
    [Fact]
    public void New_ShouldReturnNonEmptyId()
    {
        var id = PermitsId.New();
        id.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Constructor_WithEmptyGuid_ShouldThrow() =>
        FluentActions.Invoking(() => PermitsId.FromGuid(Guid.Empty))
                     .Should().Throw<ArgumentException>();

    [Fact]
    public void TwoIds_WithSameGuid_ShouldBeEqual()
    {
        var g = Guid.NewGuid();
        PermitsId.FromGuid(g).Should().Be(PermitsId.FromGuid(g));
    }

    [Fact]
    public void TwoIds_WithDifferentGuids_ShouldNotBeEqual() =>
        PermitsId.New().Should().NotBe(PermitsId.New());
}
