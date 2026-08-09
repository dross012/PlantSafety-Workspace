// ============================================================================
//  Project:        Shared.ErrorHandling
//  Module:         Abstractions.Policy
//  File:           CircuitBreaker.cs
//  Created:        2026-08-09
//  Description:    Phase 2.3 deliverable: circuit breaker rules.
//                  Deterministic (IClock-backed, Phase 2.1) consecutive-
//                  failure tracker with a half-open probe state, per the
//                  standard circuit breaker pattern: Closed (normal) ->
//                  Open (failing fast) once FailureThreshold consecutive
//                  failures are recorded -> HalfOpen (one probe allowed)
//                  once OpenDuration has elapsed -> Closed again on a
//                  successful probe, or back to Open on a failed one.
// ============================================================================
using Shared.Deterministic.Abstractions.Clock;

namespace Shared.ErrorHandling.Abstractions.Policy;

/// <summary>The circuit breaker's current state.</summary>
public enum CircuitState
{
    /// <summary>Normal operation; calls are allowed.</summary>
    Closed,

    /// <summary>Tripped; calls should fail fast without attempting the dependency.</summary>
    Open,

    /// <summary>OpenDuration has elapsed; one probe call is allowed to test recovery.</summary>
    HalfOpen,
}

/// <summary>A deterministic, per-dependency circuit breaker.</summary>
public sealed class CircuitBreaker(CircuitBreakerPolicy policy, IClock clock)
{
    private int _consecutiveFailures;
    private DateTime? _openedAtUtc;

    /// <summary>The circuit's current state.</summary>
    public CircuitState State
    {
        get
        {
            if (_openedAtUtc is not { } openedAt)
            {
                return CircuitState.Closed;
            }

            return clock.UtcNow - openedAt >= policy.OpenDuration ? CircuitState.HalfOpen : CircuitState.Open;
        }
    }

    /// <summary>Records a successful call, closing the circuit and resetting the failure count.</summary>
    public void RecordSuccess()
    {
        _consecutiveFailures = 0;
        _openedAtUtc = null;
    }

    /// <summary>Records a failed call. Trips the circuit if FailureThreshold is reached, or immediately reopens it if a HalfOpen probe just failed.</summary>
    public void RecordFailure()
    {
        if (State == CircuitState.HalfOpen)
        {
            _openedAtUtc = clock.UtcNow;
            return;
        }

        _consecutiveFailures++;
        if (_consecutiveFailures >= policy.FailureThreshold)
        {
            _openedAtUtc = clock.UtcNow;
        }
    }
}
