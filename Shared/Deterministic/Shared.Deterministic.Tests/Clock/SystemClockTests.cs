// ============================================================================
//  Project:        Shared.Deterministic.Tests
//  File:           SystemClockTests.cs
//  Created:        2026-08-09
// ============================================================================
using AwesomeAssertions;

using Shared.Deterministic.Abstractions.Clock;

using Xunit;

namespace Shared.Deterministic.Tests.Clock;

public sealed class SystemClockTests
{
    [Fact]
    public void UtcNow_IsWithinASecondOfTheRealWallClock()
    {
        IClock clock = new SystemClock();

        clock.UtcNow.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void UtcNow_HasUtcKind()
    {
        IClock clock = new SystemClock();

        clock.UtcNow.Kind.Should().Be(DateTimeKind.Utc);
    }
}
