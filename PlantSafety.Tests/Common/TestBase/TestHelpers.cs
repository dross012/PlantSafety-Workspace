// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Common
//  Layer:          Tests (Unit)
//  File:           TestHelpers.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Static utility methods shared across all unit tests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
namespace PlantSafety.Tests.Common.TestBase;

/// <summary>
/// Static utility methods for unit tests.
/// All methods are deterministic and side-effect free.
/// </summary>
public static class TestHelpers
{
    /// <summary>Returns a deterministic <see cref="CorrelationId"/> for use in tests.</summary>
    public static CorrelationId NewCorrelationId() =>
        CorrelationId.FromGuid(new Guid("00000003-0000-0000-0000-000000000003"));

    /// <summary>Returns a deterministic <see cref="CausationId"/> for use in tests.</summary>
    public static CausationId NewCausationId() =>
        CausationId.FromGuid(new Guid("00000004-0000-0000-0000-000000000004"));

    /// <summary>Returns the canonical frozen <see cref="UtcTimestamp"/> for assertions.</summary>
    public static UtcTimestamp FrozenNow() =>
        UtcTimestamp.From(new DateTime(2026, 6, 17, 12, 0, 0, DateTimeKind.Utc));

    /// <summary>
    /// Asserts that invoking <paramref name="action"/> throws a <typeparamref name="TException"/>.
    /// </summary>
    public static TException ShouldThrow<TException>(Action action) where TException : Exception
    {
        var ex = Record.Exception(action);
        ex.Should().NotBeNull().And.BeOfType<TException>();
        return (TException)ex!;
    }
}
