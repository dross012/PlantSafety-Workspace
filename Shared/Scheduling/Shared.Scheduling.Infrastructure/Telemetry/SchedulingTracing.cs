// ============================================================================
//  Project:        Shared.Scheduling.Infrastructure
//  Module:         Telemetry
//  File:           SchedulingTracing.cs
//  Created:        2026-08-10
//  Description:    Phase 14 (Scheduling Modernization) deliverable.
//                  Deterministic span recording built entirely on
//                  Shared.Telemetry's existing ILogEventWriter/LogEvent and
//                  Shared.Deterministic's IClock -- no new tracing
//                  primitive, no OpenTelemetry/Activity dependency (this
//                  program's rule handbook requires pure C# only, no
//                  external dependencies). A "span" here is simply a
//                  structured log event carrying a name and a
//                  clock-derived duration, exactly the pattern
//                  ReliabilityHooks (Phase 8) already established for
//                  operation-level timing.
// ============================================================================
using Shared.Scheduling.Infrastructure.Governance;
using Shared.Telemetry.Abstractions.Events;

namespace Shared.Scheduling.Infrastructure.Telemetry;

/// <summary>Deterministic, clock-derived span recording for Scheduling orchestration steps.</summary>
public static class SchedulingTracing
{
    private const string Module = "Scheduling";

    /// <summary>
    /// Writes one SCHEDULING_TRACE_SPAN log event describing <paramref name="spanName"/>'s duration
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
            EventId: SchedulingGovernanceEvents.TraceSpan,
            Category: LogCategory.Scheduling,
            Severity: LogSeverity.Debug,
            Message: $"Scheduling span \"{spanName}\" completed in {durationMs:F1}ms.",
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
