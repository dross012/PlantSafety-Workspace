// ============================================================================
//  Project:        Shared.Telemetry
//  Module:         Abstractions.Events
//  File:           ILogEventWriter.cs
//  Created:        2026-08-09
//  Description:    Phase 2.4 deliverable: the logging adapter layer's
//                  seam. The one approved way to emit a structured log
//                  event -- callers build an ILogEvent (typically via
//                  LogEventWriter's Write helpers) instead of calling
//                  ILogger directly with ad-hoc message templates.
// ============================================================================
namespace Shared.Telemetry.Abstractions.Events;

/// <summary>The one approved seam for emitting a structured log event.</summary>
public interface ILogEventWriter
{
    /// <summary>Writes the given structured log event.</summary>
    void Write(ILogEvent logEvent);
}
