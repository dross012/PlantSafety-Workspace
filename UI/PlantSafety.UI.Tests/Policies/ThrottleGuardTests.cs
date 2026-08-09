// ============================================================================
//  Project:        PlantSafety.UI.Tests
//  File:           ThrottleGuardTests.cs
//  Created:        2026-08-09
//  Description:    Tests for Phase 2.2's ThrottleGuard using a fake IClock
//                  (no real Thread.Sleep/timing flakiness).
// ============================================================================
using AwesomeAssertions;

using PlantSafety.UI.Policies;
using PlantSafety.UI.Services;

using Xunit;

namespace PlantSafety.UI.Tests.Policies;

public sealed class ThrottleGuardTests
{
    private sealed class FakeClock : IClock
    {
        public DateTime UtcNow { get; set; } = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    }

    [Fact]
    public void TryEnter_FirstCall_Succeeds()
    {
        var guard = new ThrottleGuard(new FakeClock(), TimeSpan.FromSeconds(1));

        guard.TryEnter().Should().BeTrue();
    }

    [Fact]
    public void TryEnter_SecondCallWithinInterval_Fails()
    {
        var clock = new FakeClock();
        var guard = new ThrottleGuard(clock, TimeSpan.FromSeconds(1));
        guard.TryEnter();

        clock.UtcNow = clock.UtcNow.AddMilliseconds(500);

        guard.TryEnter().Should().BeFalse();
    }

    [Fact]
    public void TryEnter_SecondCallAfterInterval_Succeeds()
    {
        var clock = new FakeClock();
        var guard = new ThrottleGuard(clock, TimeSpan.FromSeconds(1));
        guard.TryEnter();

        clock.UtcNow = clock.UtcNow.AddSeconds(1.5);

        guard.TryEnter().Should().BeTrue();
    }

    [Fact]
    public void Reset_AllowsImmediateReentry()
    {
        var clock = new FakeClock();
        var guard = new ThrottleGuard(clock, TimeSpan.FromSeconds(1));
        guard.TryEnter();

        guard.Reset();

        guard.TryEnter().Should().BeTrue();
    }
}
