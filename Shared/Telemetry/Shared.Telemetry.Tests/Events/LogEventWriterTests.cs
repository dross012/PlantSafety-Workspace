// ============================================================================
//  Project:        Shared.Telemetry.Tests
//  File:           LogEventWriterTests.cs
//  Created:        2026-08-09
// ============================================================================
using AwesomeAssertions;

using Microsoft.Extensions.Logging;

using Shared.Telemetry.Abstractions.Events;

using Xunit;

namespace Shared.Telemetry.Tests.Events;

public sealed class LogEventWriterTests
{
    private sealed class CapturingLogger : ILogger<LogEventWriter>
    {
        public LogLevel? CapturedLevel { get; private set; }
        public IReadOnlyList<KeyValuePair<string, object?>>? CapturedState { get; private set; }
        public string? CapturedMessage { get; private set; }

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            CapturedLevel = logLevel;
            CapturedState = state as IReadOnlyList<KeyValuePair<string, object?>>;
            CapturedMessage = formatter(state, exception);
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();
            public void Dispose()
            {
            }
        }
    }

    [Fact]
    public void Write_MapsSeverityToTheCorrectLogLevel()
    {
        var logger = new CapturingLogger();
        var writer = new LogEventWriter(logger);
        var logEvent = new LogEvent("EVT-1", LogCategory.Facility, LogSeverity.Warning, "test message",
            new Dictionary<string, object?>(), "corr-1", "facilities", DateTime.UtcNow);

        writer.Write(logEvent);

        logger.CapturedLevel.Should().Be(LogLevel.Warning);
    }

    [Fact]
    public void Write_StateIncludesEveryEnvelopeFieldAndFlattenedPayload()
    {
        var logger = new CapturingLogger();
        var writer = new LogEventWriter(logger);
        var logEvent = new LogEvent("EVT-2", LogCategory.Machine, LogSeverity.Info, "machine created",
            new Dictionary<string, object?> { ["MachineId"] = "abc-123" }, "corr-2", "machines", DateTime.UtcNow);

        writer.Write(logEvent);

        logger.CapturedState.Should().NotBeNull();
        logger.CapturedState!.Should().Contain(kvp => kvp.Key == "EventId" && Equals(kvp.Value, "EVT-2"));
        logger.CapturedState!.Should().Contain(kvp => kvp.Key == "CorrelationId" && Equals(kvp.Value, "corr-2"));
        logger.CapturedState!.Should().Contain(kvp => kvp.Key == "Payload.MachineId" && Equals(kvp.Value, "abc-123"));
    }

    [Fact]
    public void Write_MessageIsThePlainEventMessage()
    {
        var logger = new CapturingLogger();
        var writer = new LogEventWriter(logger);
        var logEvent = new LogEvent("EVT-3", LogCategory.Employee, LogSeverity.Error, "something failed",
            new Dictionary<string, object?>(), "corr-3", "employees", DateTime.UtcNow);

        writer.Write(logEvent);

        logger.CapturedMessage.Should().Be("something failed");
    }
}
