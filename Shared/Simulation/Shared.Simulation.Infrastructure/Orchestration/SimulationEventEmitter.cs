// ============================================================================
//  Project:        Shared.Simulation.Infrastructure
//  Module:         Orchestration
//  File:           SimulationEventEmitter.cs
//  Created:        2026-08-10
//  Description:    Phase 17 deliverable. Emits a composed
//                  SimulationEvaluationResultContract as a structured
//                  LogEvent -- the unified event schema (Phase 12
//                  precedent), mirroring SafetyGraphEventEmitter (Phase 16)
//                  exactly. Pure emission: never recomputes the replay or
//                  scenario outcome.
// ============================================================================
using Shared.Simulation.Abstractions.Simulation;
using Shared.Simulation.Infrastructure.Governance;
using Shared.Telemetry.Abstractions.Events;

namespace Shared.Simulation.Infrastructure.Orchestration;

/// <summary>Emits a composed SimulationEvaluationResultContract as a structured LogEvent.</summary>
public static class SimulationEventEmitter
{
    private const string Module = "Simulation";

    /// <summary>Writes one SIMULATION_EVALUATION_EVENT_PUBLISHED log event carrying <paramref name="result"/>.</summary>
    public static void Emit(SimulationEvaluationResultContract result, ILogEventWriter logEventWriter)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(logEventWriter);

        logEventWriter.Write(new LogEvent(
            EventId: SimulationGovernanceEvents.EvaluationEventPublished,
            Category: LogCategory.Simulation,
            Severity: LogSeverity.Info,
            Message: $"Published Simulation evaluation for tenant \"{result.Metadata.TenantId}\": scenario \"{result.Outcome.Scenario.ScenarioId}\", priority {result.Outcome.OverallPriority}.",
            Payload: new Dictionary<string, object?>
            {
                ["TenantId"] = result.Metadata.TenantId,
                ["ScenarioId"] = result.Outcome.Scenario.ScenarioId,
                ["IsValidReplay"] = result.Outcome.Replay.IsValidReplay,
                ["RequiresOverallEscalation"] = result.Outcome.RequiresOverallEscalation,
                ["OverallPriority"] = result.Outcome.OverallPriority.ToString(),
            },
            CorrelationId: result.Metadata.CorrelationId,
            Module: Module,
            OccurredAtUtc: result.Metadata.IssuedAtUtc));
    }
}
