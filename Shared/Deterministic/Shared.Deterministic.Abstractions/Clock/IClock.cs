// ============================================================================
//  Project:        Shared.Deterministic
//  Module:         Abstractions.Clock
//  File:           IClock.cs
//  Created:        2026-08-09
//  Description:    Phase 2.1 (System Stabilization & Deterministic
//                  Foundations): the one approved seam for reading the
//                  current time. Call sites that need "now" take an
//                  IClock via constructor injection instead of calling
//                  DateTime.UtcNow directly, so every module's timestamped
//                  behavior can be pinned to a fixed instant under test
//                  (DeterministicModeOptions/FixedClock) while still
//                  running against the real wall clock in production
//                  (SystemClock, the only DI-registered implementation
//                  outside of tests).
// ============================================================================
namespace Shared.Deterministic.Abstractions.Clock;

/// <summary>The one approved seam for reading the current UTC time.</summary>
public interface IClock
{
    /// <summary>The current UTC instant.</summary>
    DateTime UtcNow { get; }
}
