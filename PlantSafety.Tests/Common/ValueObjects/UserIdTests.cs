// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Common.ValueObjects
//  Layer:          Tests (Unit)
//  File:           UserIdTests.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Unit tests for UserId SharedKernel value object.
// ============================================================================
namespace PlantSafety.Tests.Common.ValueObjects;

public sealed class UserIdTests : UnitTestBase
{
    [Fact]
    public void FromGuid_WithValidGuid_ShouldPreserveValue()
    {
        var g = new Guid("00000002-0000-0000-0000-000000000002");
        UserId.FromGuid(g).Value.Should().Be(g);
    }

    [Fact]
    public void FromGuid_WithEmptyGuid_ShouldThrow() =>
        FluentActions.Invoking(() => UserId.FromGuid(Guid.Empty))
                     .Should().Throw<ArgumentException>();
}
