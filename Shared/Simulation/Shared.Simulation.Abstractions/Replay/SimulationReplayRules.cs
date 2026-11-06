// ============================================================================
//  Project:        Shared.Simulation.Abstractions
//  Module:         Replay
//  File:           SimulationReplayRules.cs
//  Created:        2026-08-10
//  Description:    Phase 17 deliverable. Deterministic, pure validation of a
//                  scenario's step ordering -- the "deterministic event
//                  ordering" and "deterministic state transitions" this
//                  phase's replay requirements ask for are enforced here as
//                  a fixed contiguity/uniqueness check on StepIndex, never a
//                  wall-clock or insertion-order dependent computation.
// ============================================================================
using Shared.Simulation.Abstractions.Simulation;

namespace Shared.Simulation.Abstractions.Replay;

/// <summary>Deterministic, pure validation of a scenario's step ordering.</summary>
public static class SimulationReplayRules
{
    /// <summary>
    /// Validates that <paramref name="scenario"/>'s Steps form a zero-based, contiguous, duplicate-free
    /// StepIndex sequence. Returns every violation found (empty if valid).
    /// </summary>
    public static IReadOnlyList<string> ValidateStepOrdering(SimulationScenario scenario)
    {
        ArgumentNullException.ThrowIfNull(scenario);

        var violations = new List<string>();

        var seenIndices = new HashSet<int>();
        foreach (SimulationStep step in scenario.Steps)
        {
            if (!seenIndices.Add(step.StepIndex))
            {
                violations.Add($"Duplicate StepIndex {step.StepIndex} found -- each step must have a unique index.");
            }
        }

        List<SimulationStep> sorted = scenario.Steps.OrderBy(step => step.StepIndex).ToList();
        for (int position = 0; position < sorted.Count; position++)
        {
            if (sorted[position].StepIndex != position)
            {
                violations.Add($"Step at replay position {position} has StepIndex {sorted[position].StepIndex}, expected {position} -- steps must be zero-based and contiguous.");
            }
        }

        return violations;
    }
}
