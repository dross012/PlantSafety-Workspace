// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Common.ValueObjects
//  Layer:          Tests (Unit)
//  File:           TenantIdTests.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Unit tests for TenantId SharedKernel value object.
// ============================================================================
namespace PlantSafety.Tests.Common.ValueObjects;

public sealed class TenantIdTests : UnitTestBase
{
    [Fact]
    public void FromGuid_WithValidGuid_ShouldPreserveValue()
    {
        var g = new Guid("00000001-0000-0000-0000-000000000001");
        TenantId.FromGuid(g).Value.Should().Be(g);
    }

    [Fact]
    public void FromGuid_WithEmptyGuid_ShouldThrow() =>
        FluentActions.Invoking(() => TenantId.FromGuid(Guid.Empty))
                     .Should().Throw<ArgumentException>();

    [Fact]
    public void TwoIds_WithSameGuid_ShouldBeEqual()
    {
        var g = Guid.NewGuid();
        TenantId.FromGuid(g).Should().Be(TenantId.FromGuid(g));
    }
}
