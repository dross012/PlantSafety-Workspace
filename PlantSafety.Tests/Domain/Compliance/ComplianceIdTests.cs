// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Compliance.Compliance
//  Layer:          Tests (Unit)
//  File:           ComplianceIdTests.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Unit tests for ComplianceId StrongId value object.
// ============================================================================
namespace PlantSafety.Tests.Compliance.Compliance;

public sealed class ComplianceIdTests : UnitTestBase
{
    [Fact]
    public void New_ShouldReturnNonEmptyId()
    {
        var id = ComplianceId.New();
        id.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Constructor_WithEmptyGuid_ShouldThrow() =>
        FluentActions.Invoking(() => ComplianceId.FromGuid(Guid.Empty))
                     .Should().Throw<ArgumentException>();

    [Fact]
    public void TwoIds_WithSameGuid_ShouldBeEqual()
    {
        var g = Guid.NewGuid();
        ComplianceId.FromGuid(g).Should().Be(ComplianceId.FromGuid(g));
    }

    [Fact]
    public void TwoIds_WithDifferentGuids_ShouldNotBeEqual() =>
        ComplianceId.New().Should().NotBe(ComplianceId.New());
}
