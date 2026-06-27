// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Common.ValueObjects
//  Layer:          Tests (Unit)
//  File:           CorrelationIdTests.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Unit tests for CorrelationId SharedKernel value object.
// ============================================================================
namespace PlantSafety.Tests.Common.ValueObjects;

public sealed class CorrelationIdTests : UnitTestBase
{
    [Fact]
    public void FromGuid_WithValidGuid_ShouldPreserveValue()
    {
        var g = new Guid("00000003-0000-0000-0000-000000000003");
        CorrelationId.FromGuid(g).Value.Should().Be(g);
    }

    [Fact]
    public void FromGuid_WithEmptyGuid_ShouldThrow() =>
        FluentActions.Invoking(() => CorrelationId.FromGuid(Guid.Empty))
                     .Should().Throw<ArgumentException>();

    [Fact]
    public void TwoIds_WithSameGuid_ShouldBeEqual()
    {
        var g = Guid.NewGuid();
        CorrelationId.FromGuid(g).Should().Be(CorrelationId.FromGuid(g));
    }
}
