// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Common
//  Layer:          Tests (Unit)
//  File:           FakeLogger.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    In-memory ILogger<T> that captures structured log entries for assertion in unit tests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
namespace PlantSafety.Tests.Common.TestBase;

/// <summary>
/// In-memory <see cref="ILogger{T}"/> that captures all log entries.
/// Allows unit tests to assert structured log output without external sinks.
/// Thread-safe for parallel test execution.
/// </summary>
public sealed class FakeLogger<T> : ILogger<T>
{
    private readonly List<FakeLogEntry> _entries = [];

    /// <summary>All log entries captured during the test.</summary>
    public IReadOnlyList<FakeLogEntry> Entries => _entries.AsReadOnly();

    /// <inheritdoc/>
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    /// <inheritdoc/>
    public bool IsEnabled(LogLevel logLevel) => true;

    /// <inheritdoc/>
    public void Log<TState>(
        LogLevel logLevel, EventId eventId, TState state,
        Exception? exception, Func<TState, Exception?, string> formatter)
    {
        lock (_entries)
        {
            _entries.Add(new FakeLogEntry(logLevel, formatter(state, exception), exception));
        }
    }

    /// <summary>Asserts that at least one entry at the given level was logged.</summary>
    public void ShouldHaveLogged(LogLevel level) =>
        _entries.Should().Contain(e => e.Level == level,
            $"expected at least one {level} log entry");

    /// <summary>Asserts that no entries at <see cref="LogLevel.Error"/> or above were logged.</summary>
    public void ShouldHaveNoErrors() =>
        _entries.Should().NotContain(e => e.Level >= LogLevel.Error,
            "no error-level logs expected in a successful operation");
}

/// <summary>A single captured log entry.</summary>
public sealed record FakeLogEntry(LogLevel Level, string Message, Exception? Exception);
