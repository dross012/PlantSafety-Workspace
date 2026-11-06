// ============================================================================
//  Project:        Shared.Simulation.Abstractions
//  Module:         Replay
//  File:           SimulationReplayEngine.cs
//  Created:        2026-08-10
//  Description:    Phase 17 deliverable. The public, pure entry point for a
//                  scenario replay: validates step ordering via
//                  SimulationReplayRules, then replays every step in
//                  ascending StepIndex order (the one deterministic state
//                  transition this engine performs -- "state" here is
//                  simply "which step comes next", since Simulation invents
//                  no new domain state beyond what SimulationScenarioEngine
//                  already evaluates). Never invents new domain semantics:
//                  a replay is a validated re-ordering of the scenario's own
//                  steps, not a re-derivation of their outcomes.
// ============================================================================
using Shared.Simulation.Abstractions.Simulation;

namespace Shared.Simulation.Abstractions.Replay;

/// <summary>Deterministically replays a scenario's steps in validated, ascending StepIndex order.</summary>
public static class SimulationReplayEngine
{
    /// <summary>Replays <paramref name="scenario"/>. Same input always produces the same output.</summary>
    public static SimulationReplayResult Replay(SimulationScenario scenario)
    {
        ArgumentNullException.ThrowIfNull(scenario);

        IReadOnlyList<string> violations = SimulationReplayRules.ValidateStepOrdering(scenario);

        IReadOnlyList<int> replayedStepIndices = scenario.Steps
            .OrderBy(step => step.StepIndex)
            .Select(step => step.StepIndex)
            .ToArray();

        return new SimulationReplayResult(
            ScenarioId: scenario.ScenarioId,
            ReplayedStepIndices: replayedStepIndices,
            IsValidReplay: violations.Count == 0,
            AppliedRuleCodes: violations);
    }
}
