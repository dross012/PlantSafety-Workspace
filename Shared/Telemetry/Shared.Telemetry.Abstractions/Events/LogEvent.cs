// ============================================================================
//  Project:        Shared.Telemetry
//  Module:         Abstractions.Events
//  File:           LogEvent.cs
//  Created:        2026-08-09
//  Description:    The concrete, immutable ILogEvent implementation every
//                  ILogEventWriter call constructs.
// ============================================================================
namespace Shared.Telemetry.Abstractions.Events;

/// <summary>The concrete, immutable ILogEvent implementation.</summary>
public sealed record LogEvent(
    string EventId,
    LogCategory Category,
    LogSeverity Severity,
    string Message,
    IReadOnlyDictionary<string, object?> Payload,
    string CorrelationId,
    string Module,
    DateTime OccurredAtUtc) : ILogEvent;
