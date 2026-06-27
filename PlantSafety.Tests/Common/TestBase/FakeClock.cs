// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Common
//  Layer:          Tests (Unit)
//  File:           FakeClock.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Deterministic fake clock frozen to a fixed UTC instant for unit tests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
namespace PlantSafety.Tests.Common.TestBase;

/// <summary>
/// Deterministic fake clock. Frozen by default to <see cref="UnitTestBase.FixedUtcNow"/>.
/// Advance with <see cref="AdvanceBy"/> in scenarios that need time progression.
/// Never uses <c>DateTime.UtcNow</c> — always deterministic.
/// </summary>
public sealed class FakeClock
{
    private DateTime _current = new(2026, 6, 17, 12, 0, 0, DateTimeKind.Utc);

    /// <summary>Current frozen UTC instant.</summary>
    public DateTime UtcNow => _current;

    /// <summary>Current instant as <see cref="UtcTimestamp"/>.</summary>
    public UtcTimestamp Now() => UtcTimestamp.From(_current);

    /// <summary>Advances the clock by the specified duration.</summary>
    public void AdvanceBy(TimeSpan delta) => _current = _current.Add(delta);

    /// <summary>Resets the clock to the canonical fixed instant.</summary>
    public void Reset() => _current = new(2026, 6, 17, 12, 0, 0, DateTimeKind.Utc);
}
