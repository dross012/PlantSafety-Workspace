// ============================================================================
//  Project:        Shared.Telemetry
//  Module:         Abstractions.Events
//  File:           LogEventWriter.cs
//  Created:        2026-08-09
//  Description:    The logging adapter layer: wraps Microsoft.Extensions.
//                  Logging.ILogger, writing every ILogEvent's fields as
//                  structured state (not a formatted message string) so a
//                  JSON-formatting provider (Program.cs's
//                  builder.Logging.AddJsonConsole()) emits genuine
//                  structured JSON with every field as its own property,
//                  not a JSON string with values already baked into text.
// ============================================================================
using Microsoft.Extensions.Logging;

namespace Shared.Telemetry.Abstractions.Events;

/// <summary>Writes ILogEvents as structured state through Microsoft.Extensions.Logging.</summary>
public sealed class LogEventWriter(ILogger<LogEventWriter> logger) : ILogEventWriter
{
    /// <inheritdoc />
    public void Write(ILogEvent logEvent)
    {
        var state = new List<KeyValuePair<string, object?>>
        {
            new("EventId", logEvent.EventId),
            new("Category", logEvent.Category.ToString()),
            new("Module", logEvent.Module),
            new("CorrelationId", logEvent.CorrelationId),
            new("OccurredAtUtc", logEvent.OccurredAtUtc),
        };
        foreach ((string key, object? value) in logEvent.Payload)
        {
            state.Add(new KeyValuePair<string, object?>($"Payload.{key}", value));
        }

        logger.Log(ToLogLevel(logEvent.Severity), new EventId(0, logEvent.EventId), state, exception: null, (s, _) => logEvent.Message);
    }

    private static LogLevel ToLogLevel(LogSeverity severity) => severity switch
    {
        LogSeverity.Trace => LogLevel.Trace,
        LogSeverity.Debug => LogLevel.Debug,
        LogSeverity.Info => LogLevel.Information,
        LogSeverity.Warning => LogLevel.Warning,
        LogSeverity.Error => LogLevel.Error,
        LogSeverity.Critical => LogLevel.Critical,
        _ => LogLevel.Information,
    };
}
