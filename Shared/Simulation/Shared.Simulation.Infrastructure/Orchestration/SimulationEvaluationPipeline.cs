// ============================================================================
//  Project:        Shared.Simulation.Infrastructure
//  Module:         Orchestration
//  File:           SimulationEvaluationPipeline.cs
//  Created:        2026-08-10
//  Description:    Phase 17 deliverable. The deterministic composition step
//                  that replays a scenario, then evaluates it, in that fixed
//                  order: Replay -> Evaluate. Mirrors SafetyGraphEvaluationPipeline
//                  (Phase 16, Construct -> Infer) exactly. OverallPriority/
//                  RequiresOverallEscalation are worst-case-wins across the
//                  replay's validity and the scenario's own escalation --
//                  an invalid replay (IsValidReplay: false) always drives
//                  RequiresOverallEscalation to true, regardless of the
//                  scenario's own outcome.
// ============================================================================
using Shared.Deterministic.Abstractions.Clock;
using Shared.PredictiveSafety.Abstractions.Common;
using Shared.Simulation.Abstractions.Replay;
using Shared.Simulation.Abstractions.Scenario;
using Shared.Simulation.Abstractions.Simulation;
using Shared.Simulation.Infrastructure.Telemetry;
using Shared.Telemetry.Abstractions.Events;

namespace Shared.Simulation.Infrastructure.Orchestration;

/// <summary>Deterministically composes scenario replay and scenario evaluation, in fixed order.</summary>
public static class SimulationEvaluationPipeline
{
    /// <summary>Evaluates <paramref name="request"/>: replays the scenario, then evaluates it, in that fixed order.</summary>
    public static async Task<SimulationEvaluationResultContract> EvaluateAsync(
        SimulationEvaluationRequestContract request,
        IClock clock,
        ILogEventWriter logEventWriter)
    {
        ArgumentNullException.ThrowIfNull(request);

        string correlationId = request.Metadata.CorrelationId;

        SimulationReplayResult replay = await SimulationTelemetry.ReplayWithTelemetryAsync(request.Scenario, clock, logEventWriter, correlationId);

        var scenarioContext = new SimulationScenarioContext(request.Scenario, request.PredictiveResult, request.Graph);
        SimulationScenarioResult scenario = await SimulationTelemetry.EvaluateWithTelemetryAsync(scenarioContext, clock, logEventWriter, correlationId);

        bool requiresOverallEscalation = !replay.IsValidReplay || scenario.RequiresScenarioWideEscalation;
        PredictivePriority overallPriority = scenario.OverallPriority;
        if (!replay.IsValidReplay && overallPriority < PredictivePriority.Critical)
        {
            overallPriority = PredictivePriority.Critical;
        }

        var outcome = new SimulationOutcome(replay, scenario, requiresOverallEscalation, overallPriority);

        return new SimulationEvaluationResultContract(request.Metadata, outcome);
    }
}
