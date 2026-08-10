// ============================================================================
//  Project:        Shared.PredictiveSafety.Infrastructure
//  Module:         Telemetry
//  File:           PredictiveTracing.cs
//  Created:        2026-08-10
//  Description:    Phase 15 deliverable. Deterministic span recording built
//                  entirely on Shared.Telemetry's existing ILogEventWriter/
//                  LogEvent and Shared.Deterministic's IClock -- no new
//                  tracing primitive, mirroring SchedulingTracing (Phase 14)
//                  exactly.
// ============================================================================
using Shared.PredictiveSafety.Infrastructure.Governance;
using Shared.Telemetry.Abstractions.Events;

namespace Shared.PredictiveSafety.Infrastructure.Telemetry;

/// <summary>Deterministic, clock-derived span recording for Predictive Safety orchestration steps.</summary>
public static class PredictiveTracing
{
    private const string Module = "PredictiveSafety";

    /// <summary>
    /// Writes one PREDICTIVE_TRACE_SPAN log event describing <paramref name="spanName"/>'s duration
    /// between <paramref name="startedAtUtc"/> and <paramref name="endedAtUtc"/>. Never throws, never
    /// alters the caller's control flow -- pure observability.
    /// </summary>
    public static void RecordSpan(
        string spanName,
        DateTime startedAtUtc,
        DateTime endedAtUtc,
        string correlationId,
        ILogEventWriter logEventWriter)
    {
        ArgumentNullException.ThrowIfNull(spanName);
        ArgumentNullException.ThrowIfNull(logEventWriter);

        double durationMs = (endedAtUtc - startedAtUtc).TotalMilliseconds;

        logEventWriter.Write(new LogEvent(
            EventId: PredictiveGovernanceEvents.TraceSpan,
            Category: LogCategory.PredictiveSafety,
            Severity: LogSeverity.Debug,
            Message: $"Predictive Safety span \"{spanName}\" completed in {durationMs:F1}ms.",
            Payload: new Dictionary<string, object?>
            {
                ["SpanName"] = spanName,
                ["DurationMs"] = durationMs,
            },
            CorrelationId: correlationId,
            Module: Module,
            OccurredAtUtc: endedAtUtc));
    }
}
