// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Setup
//  Layer:          IntegrationTests
//  File:           TestOutputWriter.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    ILogger sink that routes structured log output to the xunit ITestOutputHelper.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
namespace PlantSafety.IntegrationTests.Setup;

/// <summary>
/// Routes <see cref="ILogger"/> structured output to the xunit v3
/// <see cref="ITestOutputHelper"/> so every test failure includes its log context.
/// </summary>
public sealed class XunitLoggerProvider(ITestOutputHelper output) : ILoggerProvider
{
    /// <inheritdoc/>
    public ILogger CreateLogger(string categoryName) => new XunitLogger(output, categoryName);

    /// <inheritdoc/>
    public void Dispose() { }

    private sealed class XunitLogger(ITestOutputHelper output, string categoryName) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;

        public void Log<TState>(
            LogLevel logLevel, EventId eventId, TState state,
            Exception? exception, Func<TState, Exception?, string> formatter)
        {
            output.WriteLine($"[{logLevel,-11}] [{categoryName}] {formatter(state, exception)}");
            if (exception is not null)
                output.WriteLine($"  ExceptionType={exception.GetType().Name} Message={exception.Message}");
        }
    }
}
