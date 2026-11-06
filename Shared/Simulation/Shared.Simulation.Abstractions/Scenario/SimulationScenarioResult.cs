// ============================================================================
//  Project:        Shared.Simulation.Abstractions
//  Module:         Scenario
//  File:           SimulationScenarioResult.cs
//  Created:        2026-08-10
//  Description:    Phase 17 deliverable. The deterministic output of a full
//                  scenario evaluation. OverallPriority reuses Shared.
//                  PredictiveSafety.Abstractions.Common.PredictivePriority
//                  (Phase 15) rather than inventing a second priority
//                  vocabulary -- the same reuse Shared.SafetyGraph's
//                  SafetyGraphInferenceResult (Phase 16) already established.
// ============================================================================
using Shared.PredictiveSafety.Abstractions.Common;
using Shared.Simulation.Abstractions.Simulation;

namespace Shared.Simulation.Abstractions.Scenario;

/// <summary>A single step's deterministic evaluation outcome within a scenario.</summary>
public sealed record SimulationStepOutcome(
    int StepIndex,
    SimulationStepType StepType,
    bool RequiresEscalation,
    PredictivePriority Priority,
    IReadOnlyList<string> AppliedRuleCodes);

/// <summary>The deterministic output of a full scenario evaluation.</summary>
public sealed record SimulationScenarioResult(
    string ScenarioId,
    IReadOnlyList<SimulationStepOutcome> StepOutcomes,
    bool RequiresScenarioWideEscalation,
    PredictivePriority OverallPriority,
    IReadOnlyList<string> AppliedRuleCodes);
