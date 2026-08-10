// ============================================================================
//  Project:        Shared.Simulation.Abstractions
//  Module:         Simulation
//  File:           SimulationStep.cs
//  Created:        2026-08-10
//  Description:    Phase 17 (Simulation Engine) deliverable. The unified
//                  step-type vocabulary a scenario is composed from --
//                  mirrors exactly the domains this program's prior phases
//                  already modernized (Hazard/Maintenance: Phase 15;
//                  Routing: Phase 13; Scheduling: Phase 14; Workflow: Phase
//                  C.7/16 traversal; PredictiveInference: Phase 15's
//                  composed OverallPriority). StepIndex is the deterministic,
//                  caller-supplied ordering key SimulationReplayEngine
//                  validates and SimulationScenarioEngine walks in ascending
//                  order -- never re-derived from wall-clock or insertion
//                  order.
// ============================================================================
namespace Shared.Simulation.Abstractions.Simulation;

/// <summary>The unified step-type vocabulary a simulation scenario is composed from.</summary>
public enum SimulationStepType
{
    /// <summary>A hazard-domain step (Phase 15's PredictiveHazardDecision).</summary>
    Hazard,

    /// <summary>A maintenance-domain step (Phase 15's PredictiveMaintenanceDecision).</summary>
    Maintenance,

    /// <summary>A routing-domain step (Phase 15's PredictiveRoutingDecision, Phase 13's EventRoutingResult).</summary>
    Routing,

    /// <summary>A scheduling-domain step (Phase 15's PredictiveSchedulingDecision, Phase 14's SchedulingRuleDecision).</summary>
    Scheduling,

    /// <summary>A workflow-domain step, evaluated via SafetyGraph traversal (Phase 16) from Workflow nodes.</summary>
    Workflow,

    /// <summary>A predictive-inference step (Phase 15's composed PredictiveSafetyEvaluationResultContract.OverallPriority).</summary>
    PredictiveInference,
}

/// <summary>A single, immutable step in a simulation scenario.</summary>
public sealed record SimulationStep(
    int StepIndex,
    SimulationStepType StepType,
    string Description);
