// ============================================================================
//  Project:        Deployment
//  Module:         Observability
//  File:           DeploymentTracing.cs
//  Created:        2026-08-10
//  Description:    Phase 18 deliverable. Deterministic span recording built
//                  entirely on Shared.Telemetry's existing ILogEventWriter/
//                  LogEvent and Shared.Deterministic's IClock -- no new
//                  tracing primitive, mirroring every prior phase's
//                  *Tracing.cs exactly.
// ============================================================================
using Deployment.Governance;

using Shared.Telemetry.Abstractions.Events;

namespace Deployment.Observability;

/// <summary>Deterministic, clock-derived span recording for Deployment orchestration steps.</summary>
public static class DeploymentTracing
{
    private const string Module = "Deployment";

    /// <summary>
    /// Writes one DEPLOYMENT_TRACE_SPAN log event describing <paramref name="spanName"/>'s duration
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
            EventId: GovernanceEvents.TraceSpan,
            Category: LogCategory.Deployment,
            Severity: LogSeverity.Debug,
            Message: $"Deployment span \"{spanName}\" completed in {durationMs:F1}ms.",
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
