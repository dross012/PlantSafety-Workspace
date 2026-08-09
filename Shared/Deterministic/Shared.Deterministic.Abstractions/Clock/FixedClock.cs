// ============================================================================
//  Project:        Shared.Deterministic
//  Module:         Abstractions.Clock
//  File:           FixedClock.cs
//  Created:        2026-08-09
//  Description:    A deterministic IClock that always returns the same
//                  fixed instant, or a caller-advanced one. Used by
//                  DeterministicMode integration tests and by any unit
//                  test asserting on timestamped output.
// ============================================================================
namespace Shared.Deterministic.Abstractions.Clock;

/// <summary>A deterministic clock pinned to a fixed (and optionally caller-advanced) instant.</summary>
public sealed class FixedClock(DateTime initialUtcNow) : IClock
{
    private DateTime _utcNow = initialUtcNow;

    /// <inheritdoc />
    public DateTime UtcNow => _utcNow;

    /// <summary>Advances the clock by the given amount, returning the new instant.</summary>
    public DateTime Advance(TimeSpan by) => _utcNow += by;

    /// <summary>Sets the clock to an explicit instant.</summary>
    public void Set(DateTime utcNow) => _utcNow = utcNow;
}
