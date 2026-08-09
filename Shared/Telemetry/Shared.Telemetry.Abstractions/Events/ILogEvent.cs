// ============================================================================
//  Project:        Shared.Telemetry
//  Module:         Abstractions.Events
//  File:           ILogEvent.cs
//  Created:        2026-08-09
//  Description:    Phase 2.4 deliverable: the structured log event schema
//                  every module's log entries are standardized into.
// ============================================================================
namespace Shared.Telemetry.Abstractions.Events;

/// <summary>The structured shape every log entry is standardized into.</summary>
public interface ILogEvent
{
    /// <summary>A stable identifier for this kind of event (e.g. "FACILITY_CREATED"), not a per-occurrence id.</summary>
    string EventId { get; }

    /// <summary>The module this event originated from.</summary>
    LogCategory Category { get; }

    /// <summary>The event's severity.</summary>
    LogSeverity Severity { get; }

    /// <summary>A human-readable summary of what happened.</summary>
    string Message { get; }

    /// <summary>Structured, JSON-serializable event-specific data.</summary>
    IReadOnlyDictionary<string, object?> Payload { get; }

    /// <summary>The correlation id tying this event to the request/session that produced it.</summary>
    string CorrelationId { get; }

    /// <summary>The specific module/route name (e.g. "severity-assessment"), narrower than Category.</summary>
    string Module { get; }

    /// <summary>When the event occurred, in UTC.</summary>
    DateTime OccurredAtUtc { get; }
}
