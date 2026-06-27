// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Compliance.Policy
//  Layer:          Tests (Unit)
//  File:           PolicyIdTests.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Unit tests for PolicyId StrongId value object.
// ============================================================================
namespace PlantSafety.Tests.Compliance.Policy;

public sealed class PolicyIdTests : UnitTestBase
{
    [Fact]
    public void New_ShouldReturnNonEmptyId()
    {
        var id = PolicyId.New();
        id.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Constructor_WithEmptyGuid_ShouldThrow() =>
        FluentActions.Invoking(() => PolicyId.FromGuid(Guid.Empty))
                     .Should().Throw<ArgumentException>();

    [Fact]
    public void TwoIds_WithSameGuid_ShouldBeEqual()
    {
        var g = Guid.NewGuid();
        PolicyId.FromGuid(g).Should().Be(PolicyId.FromGuid(g));
    }

    [Fact]
    public void TwoIds_WithDifferentGuids_ShouldNotBeEqual() =>
        PolicyId.New().Should().NotBe(PolicyId.New());
}
