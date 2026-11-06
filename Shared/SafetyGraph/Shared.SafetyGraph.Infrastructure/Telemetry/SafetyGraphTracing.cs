// ============================================================================
//  Project:        Shared.SafetyGraph.Infrastructure
//  Module:         Telemetry
//  File:           SafetyGraphTracing.cs
//  Created:        2026-08-10
//  Description:    Phase 16 deliverable. Deterministic span recording built
//                  entirely on Shared.Telemetry's existing ILogEventWriter/
//                  LogEvent and Shared.Deterministic's IClock -- no new
//                  tracing primitive, mirroring SchedulingTracing/
//                  PredictiveTracing (Phases 14/15) exactly.
// ============================================================================
using Shared.SafetyGraph.Infrastructure.Governance;
using Shared.Telemetry.Abstractions.Events;

namespace Shared.SafetyGraph.Infrastructure.Telemetry;

/// <summary>Deterministic, clock-derived span recording for Safety Graph orchestration steps.</summary>
public static class SafetyGraphTracing
{
    private const string Module = "SafetyGraph";

    /// <summary>
    /// Writes one SAFETY_GRAPH_TRACE_SPAN log event describing <paramref name="spanName"/>'s duration
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
            EventId: SafetyGraphGovernanceEvents.TraceSpan,
            Category: LogCategory.SafetyGraph,
            Severity: LogSeverity.Debug,
            Message: $"Safety Graph span \"{spanName}\" completed in {durationMs:F1}ms.",
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
