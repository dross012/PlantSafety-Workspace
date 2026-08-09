// ============================================================================
//  Project:        Shared.Deterministic.Tests
//  File:           FixedClockTests.cs
//  Created:        2026-08-09
// ============================================================================
using AwesomeAssertions;

using Shared.Deterministic.Abstractions.Clock;

using Xunit;

namespace Shared.Deterministic.Tests.Clock;

public sealed class FixedClockTests
{
    [Fact]
    public void UtcNow_ReturnsTheInitialInstant_UntilAdvancedOrSet()
    {
        var initial = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var clock = new FixedClock(initial);

        clock.UtcNow.Should().Be(initial);
        clock.UtcNow.Should().Be(initial);
    }

    [Fact]
    public void Advance_MovesTheClockForwardByTheGivenAmount()
    {
        var clock = new FixedClock(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        DateTime result = clock.Advance(TimeSpan.FromHours(2));

        result.Should().Be(new DateTime(2026, 1, 1, 2, 0, 0, DateTimeKind.Utc));
        clock.UtcNow.Should().Be(new DateTime(2026, 1, 1, 2, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void Set_ReplacesTheCurrentInstant()
    {
        var clock = new FixedClock(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        var target = new DateTime(2030, 6, 15, 8, 30, 0, DateTimeKind.Utc);

        clock.Set(target);

        clock.UtcNow.Should().Be(target);
    }
}
