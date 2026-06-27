// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Common.ValueObjects
//  Layer:          Tests (Unit)
//  File:           UtcTimestampTests.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Unit tests for UtcTimestamp SharedKernel value type.
// ============================================================================
namespace PlantSafety.Tests.Common.ValueObjects;

public sealed class UtcTimestampTests : UnitTestBase
{
    [Fact]
    public void From_ShouldPreserveDateTime()
    {
        var dt = new DateTime(2026, 6, 17, 12, 0, 0, DateTimeKind.Utc);
        UtcTimestamp.From(dt).Value.Should().Be(dt);
    }

    [Fact]
    public void From_WithLocalKind_ShouldThrow() =>
        FluentActions.Invoking(() => UtcTimestamp.From(DateTime.Now))
                     .Should().Throw<ArgumentException>("only UTC datetimes are valid");

    [Fact]
    public void TwoTimestamps_WithSameDateTime_ShouldBeEqual()
    {
        var dt = FixedUtcNow;
        UtcTimestamp.From(dt).Should().Be(UtcTimestamp.From(dt));
    }
}
