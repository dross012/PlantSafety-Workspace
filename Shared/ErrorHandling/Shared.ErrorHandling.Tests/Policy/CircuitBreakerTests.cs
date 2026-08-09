// ============================================================================
//  Project:        Shared.ErrorHandling.Tests
//  File:           CircuitBreakerTests.cs
//  Created:        2026-08-09
// ============================================================================
using AwesomeAssertions;

using Shared.Deterministic.Abstractions.Clock;
using Shared.ErrorHandling.Abstractions.Policy;

using Xunit;

namespace Shared.ErrorHandling.Tests.Policy;

public sealed class CircuitBreakerTests
{
    [Fact]
    public void State_NoFailuresYet_IsClosed()
    {
        var breaker = new CircuitBreaker(new CircuitBreakerPolicy(3, TimeSpan.FromSeconds(10)), new FixedClock(DateTime.UtcNow));

        breaker.State.Should().Be(CircuitState.Closed);
    }

    [Fact]
    public void RecordFailure_BelowThreshold_StaysClosed()
    {
        var breaker = new CircuitBreaker(new CircuitBreakerPolicy(3, TimeSpan.FromSeconds(10)), new FixedClock(DateTime.UtcNow));

        breaker.RecordFailure();
        breaker.RecordFailure();

        breaker.State.Should().Be(CircuitState.Closed);
    }

    [Fact]
    public void RecordFailure_ReachesThreshold_Opens()
    {
        var breaker = new CircuitBreaker(new CircuitBreakerPolicy(3, TimeSpan.FromSeconds(10)), new FixedClock(DateTime.UtcNow));

        breaker.RecordFailure();
        breaker.RecordFailure();
        breaker.RecordFailure();

        breaker.State.Should().Be(CircuitState.Open);
    }

    [Fact]
    public void State_AfterOpenDurationElapses_BecomesHalfOpen()
    {
        var clock = new FixedClock(DateTime.UtcNow);
        var breaker = new CircuitBreaker(new CircuitBreakerPolicy(1, TimeSpan.FromSeconds(10)), clock);
        breaker.RecordFailure();
        breaker.State.Should().Be(CircuitState.Open);

        clock.Advance(TimeSpan.FromSeconds(11));

        breaker.State.Should().Be(CircuitState.HalfOpen);
    }

    [Fact]
    public void RecordSuccess_AfterHalfOpenProbe_Closes()
    {
        var clock = new FixedClock(DateTime.UtcNow);
        var breaker = new CircuitBreaker(new CircuitBreakerPolicy(1, TimeSpan.FromSeconds(10)), clock);
        breaker.RecordFailure();
        clock.Advance(TimeSpan.FromSeconds(11));
        breaker.State.Should().Be(CircuitState.HalfOpen);

        breaker.RecordSuccess();

        breaker.State.Should().Be(CircuitState.Closed);
    }

    [Fact]
    public void RecordFailure_DuringHalfOpenProbe_ReopensImmediately()
    {
        var clock = new FixedClock(DateTime.UtcNow);
        var breaker = new CircuitBreaker(new CircuitBreakerPolicy(1, TimeSpan.FromSeconds(10)), clock);
        breaker.RecordFailure();
        clock.Advance(TimeSpan.FromSeconds(11));
        breaker.State.Should().Be(CircuitState.HalfOpen);

        breaker.RecordFailure();

        breaker.State.Should().Be(CircuitState.Open);
    }
}
