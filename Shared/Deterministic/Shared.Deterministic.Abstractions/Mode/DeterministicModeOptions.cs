// ============================================================================
//  Project:        Shared.Deterministic
//  Module:         Abstractions.Mode
//  File:           DeterministicModeOptions.cs
//  Created:        2026-08-09
//  Description:    Phase 2.1 deliverable: the DeterministicMode flag for
//                  integration tests. When enabled, DeterministicRuntime's
//                  DI extension registers FixedClock/SeededRandomProvider
//                  (pinned to FixedUtcNow/Seed) instead of
//                  SystemClock/SystemRandomProvider, so an entire test run
//                  gets fully reproducible timestamps and identities
//                  without every test hand-wiring its own fakes.
// ============================================================================
namespace Shared.Deterministic.Abstractions.Mode;

/// <summary>Toggles the deterministic runtime (fixed clock, seeded randomness) for integration tests.</summary>
public sealed record DeterministicModeOptions
{
    /// <summary>True to register the deterministic (fixed/seeded) implementations instead of the real ones.</summary>
    public bool IsEnabled { get; set; }

    /// <summary>The fixed instant FixedClock starts at when DeterministicMode is enabled.</summary>
    public DateTime FixedUtcNow { get; set; } = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>The seed SeededRandomProvider uses when DeterministicMode is enabled.</summary>
    public int Seed { get; set; } = 42;
}
