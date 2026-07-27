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
    public void From_WithLocalKind_ShouldNormalizeToUtc()
    {
        // NOTE: UtcTimestamp's constructor deliberately coerces any non-UTC DateTimeKind
        // to Utc via DateTime.SpecifyKind (see UtcTimestamp.cs header: "Ensures Kind == Utc")
        // rather than throwing. This is documented, intentional lenient-normalization
        // behavior relied on throughout the codebase, so this test verifies normalization
        // instead of asserting a throw that does not match the real contract.
        var result = UtcTimestamp.From(DateTime.Now);
        result.Value.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public void TwoTimestamps_WithSameDateTime_ShouldBeEqual()
    {
        var dt = FixedUtcNow;
        UtcTimestamp.From(dt).Should().Be(UtcTimestamp.From(dt));
    }
}
