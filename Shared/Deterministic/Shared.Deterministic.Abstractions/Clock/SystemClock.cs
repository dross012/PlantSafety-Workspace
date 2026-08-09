// ============================================================================
//  Project:        Shared.Deterministic
//  Module:         Abstractions.Clock
//  File:           SystemClock.cs
//  Created:        2026-08-09
//  Description:    The production IClock: a thin wrapper over the real
//                  wall clock. The only place DateTime.UtcNow is read
//                  directly for "current time" purposes -- every other
//                  call site should depend on IClock instead.
// ============================================================================
namespace Shared.Deterministic.Abstractions.Clock;

/// <summary>The production clock: reads the real system UTC time.</summary>
public sealed class SystemClock : IClock
{
    /// <inheritdoc />
    public DateTime UtcNow => DateTime.UtcNow;
}
