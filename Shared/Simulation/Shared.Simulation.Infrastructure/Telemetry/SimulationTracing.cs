// ============================================================================
//  Project:        Shared.Simulation.Infrastructure
//  Module:         Telemetry
//  File:           SimulationTracing.cs
//  Created:        2026-08-10
//  Description:    Phase 17 deliverable. Deterministic span recording built
//                  entirely on Shared.Telemetry's existing ILogEventWriter/
//                  LogEvent and Shared.Deterministic's IClock -- no new
//                  tracing primitive, mirroring SafetyGraphTracing (Phase
//                  16) exactly.
// ============================================================================
using Shared.Simulation.Infrastructure.Governance;
using Shared.Telemetry.Abstractions.Events;

namespace Shared.Simulation.Infrastructure.Telemetry;

/// <summary>Deterministic, clock-derived span recording for Simulation orchestration steps.</summary>
public static class SimulationTracing
{
    private const string Module = "Simulation";

    /// <summary>
    /// Writes one SIMULATION_TRACE_SPAN log event describing <paramref name="spanName"/>'s duration
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
            EventId: SimulationGovernanceEvents.TraceSpan,
            Category: LogCategory.Simulation,
            Severity: LogSeverity.Debug,
            Message: $"Simulation span \"{spanName}\" completed in {durationMs:F1}ms.",
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
