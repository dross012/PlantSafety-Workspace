// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Compliance.Procedures
//  Layer:          Tests (Unit)
//  File:           ProceduresIdTests.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Unit tests for ProceduresId StrongId value object.
// ============================================================================
namespace PlantSafety.Tests.Compliance.Procedures;

public sealed class ProceduresIdTests : UnitTestBase
{
    [Fact]
    public void New_ShouldReturnNonEmptyId()
    {
        var id = ProceduresId.New();
        id.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Constructor_WithEmptyGuid_ShouldThrow() =>
        FluentActions.Invoking(() => ProceduresId.FromGuid(Guid.Empty))
                     .Should().Throw<ArgumentException>();

    [Fact]
    public void TwoIds_WithSameGuid_ShouldBeEqual()
    {
        var g = Guid.NewGuid();
        ProceduresId.FromGuid(g).Should().Be(ProceduresId.FromGuid(g));
    }

    [Fact]
    public void TwoIds_WithDifferentGuids_ShouldNotBeEqual() =>
        ProceduresId.New().Should().NotBe(ProceduresId.New());
}
