// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Setup
//  Layer:          IntegrationTests
//  File:           TestClock.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Deterministic test clock frozen to a fixed UTC instant for reproducible timestamps.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
namespace PlantSafety.IntegrationTests.Setup;

/// <summary>
/// Deterministic test clock.  All tests MUST use this instead of <c>DateTime.UtcNow</c>
/// to guarantee reproducibility across CI environments.
/// </summary>
public static class TestClock
{
    /// <summary>Fixed UTC instant used by all integration tests.</summary>
    public static readonly DateTime FixedUtcNow = new(2026, 6, 17, 12, 0, 0, DateTimeKind.Utc);

    /// <summary>Returns the fixed UTC instant as a <see cref="UtcTimestamp"/>.</summary>
    public static UtcTimestamp Now() => UtcTimestamp.From(FixedUtcNow);
}
