// ============================================================================
//  Project:        Shared.Simulation.Infrastructure
//  Module:         Telemetry
//  File:           SimulationTelemetry.cs
//  Created:        2026-08-10
//  Description:    Phase 17 deliverable. Wraps SimulationReplayEngine.Replay
//                  and SimulationScenarioEngine.Evaluate (both unchanged --
//                  still pure functions) with deterministic logging,
//                  metrics, and reliability reporting, via Shared.Telemetry's
//                  existing ReliabilityHooks/ILogEventWriter/PlantSafetyMetrics.
//                  Mirrors SafetyGraphTelemetry (Phase 16) exactly.
// ============================================================================
using Shared.Deterministic.Abstractions.Clock;
using Shared.Simulation.Abstractions.Replay;
using Shared.Simulation.Abstractions.Scenario;
using Shared.Simulation.Abstractions.Simulation;
using Shared.Simulation.Infrastructure.Governance;
using Shared.Telemetry.Abstractions.Events;
using Shared.Telemetry.Abstractions.Reliability;

namespace Shared.Simulation.Infrastructure.Telemetry;

/// <summary>Instruments SimulationReplayEngine.Replay and SimulationScenarioEngine.Evaluate with deterministic logging, metrics, and reliability reporting.</summary>
public static class SimulationTelemetry
{
    private const string Module = "Simulation";

    /// <summary>Replays a scenario via the real SimulationReplayEngine.Replay, recording a SimulationReplaysPerformed metric and a structured log event.</summary>
    public static async Task<SimulationReplayResult> ReplayWithTelemetryAsync(
        SimulationScenario scenario, IClock clock, ILogEventWriter logEventWriter, string correlationId)
    {
        SimulationReplayResult result = await ReliabilityHooks.ExecuteAsync(
            Module, nameof(SimulationReplayEngine.Replay), clock, logEventWriter, correlationId,
            () => Task.FromResult(SimulationReplayEngine.Replay(scenario)));

        SimulationMetrics.ReplaysPerformed.Add(1, new KeyValuePair<string, object?>("isValidReplay", result.IsValidReplay));

        logEventWriter.Write(new LogEvent(
            EventId: SimulationGovernanceEvents.ReplayPerformed,
            Category: LogCategory.Simulation,
            Severity: LogSeverity.Info,
            Message: $"SimulationReplayEngine.Replay produced {result.ReplayedStepIndices.Count} replayed step(s), IsValidReplay={result.IsValidReplay}.",
            Payload: new Dictionary<string, object?>
            {
                ["ScenarioId"] = result.ScenarioId,
                ["ReplayedStepCount"] = result.ReplayedStepIndices.Count,
                ["IsValidReplay"] = result.IsValidReplay,
            },
            CorrelationId: correlationId,
            Module: Module,
            OccurredAtUtc: clock.UtcNow));

        return result;
    }

    /// <summary>Evaluates a scenario via the real SimulationScenarioEngine.Evaluate, recording a SimulationScenariosEvaluated metric and a structured log event.</summary>
    public static async Task<SimulationScenarioResult> EvaluateWithTelemetryAsync(
        SimulationScenarioContext context, IClock clock, ILogEventWriter logEventWriter, string correlationId)
    {
        SimulationScenarioResult result = await ReliabilityHooks.ExecuteAsync(
            Module, nameof(SimulationScenarioEngine.Evaluate), clock, logEventWriter, correlationId,
            () => Task.FromResult(SimulationScenarioEngine.Evaluate(context)));

        SimulationMetrics.ScenariosEvaluated.Add(1, new KeyValuePair<string, object?>("requiresEscalation", result.RequiresScenarioWideEscalation));

        logEventWriter.Write(new LogEvent(
            EventId: SimulationGovernanceEvents.ScenarioEvaluated,
            Category: LogCategory.Simulation,
            Severity: LogSeverity.Info,
            Message: $"SimulationScenarioEngine.Evaluate produced priority {result.OverallPriority} across {result.StepOutcomes.Count} step(s).",
            Payload: new Dictionary<string, object?>
            {
                ["ScenarioId"] = result.ScenarioId,
                ["StepCount"] = result.StepOutcomes.Count,
                ["RequiresScenarioWideEscalation"] = result.RequiresScenarioWideEscalation,
                ["OverallPriority"] = result.OverallPriority.ToString(),
            },
            CorrelationId: correlationId,
            Module: Module,
            OccurredAtUtc: clock.UtcNow));

        return result;
    }
}
