// ============================================================================
//  Project:        PlantSafety.UI
//  Module:         Policies
//  File:           ThrottleGuard.cs
//  Created:        2026-08-09
//  Description:    Phase 2.2 deliverable: part of the UI event policy
//                  engine's throttle rules. Rejects an invocation that
//                  arrives less than MinInterval after the last accepted
//                  one -- deterministic (backed by PlantSafety.UI's local
//                  IClock, not a reference to Shared.Deterministic), so
//                  its behavior is reproducible under test rather than
//                  depending on real wall-clock timing.
// ============================================================================
using PlantSafety.UI.Services;

namespace PlantSafety.UI.Policies;

/// <summary>Rejects invocations arriving within MinInterval of the last accepted one.</summary>
public sealed class ThrottleGuard(IClock clock, TimeSpan minInterval)
{
    private DateTime? _lastAcceptedUtc;

    /// <summary>Returns true and records this instant if enough time has passed since the last accepted call; otherwise false.</summary>
    public bool TryEnter()
    {
        DateTime now = clock.UtcNow;
        if (_lastAcceptedUtc is { } last && now - last < minInterval)
        {
            return false;
        }

        _lastAcceptedUtc = now;
        return true;
    }

    /// <summary>Clears the throttle window, so the next call is always accepted.</summary>
    public void Reset() => _lastAcceptedUtc = null;
}
