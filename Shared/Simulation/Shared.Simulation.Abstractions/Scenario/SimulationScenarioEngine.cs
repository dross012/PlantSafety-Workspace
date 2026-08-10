// ============================================================================
//  Project:        Shared.Simulation.Abstractions
//  Module:         Scenario
//  File:           SimulationScenarioEngine.cs
//  Created:        2026-08-10
//  Description:    Phase 17 deliverable. The public, pure entry point for a
//                  full scenario evaluation: walks context.Scenario.Steps in
//                  ascending StepIndex order (deterministic rule ordering,
//                  Phase 7), calls SimulationScenarioRules.EvaluateStep for
//                  each, and additively accumulates every applied rule code
//                  into a single worst-case-wins scenario-level outcome via
//                  PredictiveEscalationClassifier (Phase 15, reused
//                  unchanged).
// ============================================================================
using Shared.PredictiveSafety.Abstractions.Common;
using Shared.Simulation.Abstractions.Simulation;

namespace Shared.Simulation.Abstractions.Scenario;

/// <summary>Deterministically evaluates a full simulation scenario against its cross-module context.</summary>
public static class SimulationScenarioEngine
{
    /// <summary>Evaluates <paramref name="context"/>. Same input always produces the same output.</summary>
    public static SimulationScenarioResult Evaluate(SimulationScenarioContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var stepOutcomes = new List<SimulationStepOutcome>();
        var allAppliedRuleCodes = new List<string>();

        IEnumerable<SimulationStep> orderedSteps = context.Scenario.Steps.OrderBy(step => step.StepIndex);
        foreach (SimulationStep step in orderedSteps)
        {
            PredictiveEscalationOutcome outcome = SimulationScenarioRules.EvaluateStep(step, context.PredictiveResult, context.Graph);
            PredictivePriority stepPriority = PredictiveEscalationClassifier.Classify(outcome);

            stepOutcomes.Add(new SimulationStepOutcome(step.StepIndex, step.StepType, outcome.RequiresEscalation, stepPriority, outcome.AppliedRuleCodes));
            allAppliedRuleCodes.AddRange(outcome.AppliedRuleCodes);
        }

        var scenarioOutcome = new PredictiveEscalationOutcome(allAppliedRuleCodes.Count > 0, allAppliedRuleCodes);
        PredictivePriority overallPriority = PredictiveEscalationClassifier.Classify(scenarioOutcome);

        return new SimulationScenarioResult(
            ScenarioId: context.Scenario.ScenarioId,
            StepOutcomes: stepOutcomes,
            RequiresScenarioWideEscalation: scenarioOutcome.RequiresEscalation,
            OverallPriority: overallPriority,
            AppliedRuleCodes: allAppliedRuleCodes);
    }
}
