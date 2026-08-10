// ============================================================================
//  Project:        Shared.Simulation.Abstractions
//  Module:         Simulation
//  File:           SimulationContracts.cs
//  Created:        2026-08-10
//  Description:    Phase 17 deliverable. Unified schema request/result
//                  contracts for a full simulation evaluation. The request
//                  carries the real, upstream PredictiveSafetyEvaluationResultContract
//                  (Phase 15) and SafetyGraphModel (Phase 16) unchanged,
//                  rather than re-deriving them -- the same "raw cross-
//                  module inputs" pattern SafetyGraphConstructionRequestContract
//                  (Phase 16) already established for its own upstream
//                  dependencies.
// ============================================================================
using Shared.PredictiveSafety.Abstractions.Contracts;
using Shared.SafetyGraph.Abstractions.Graph;

namespace Shared.Simulation.Abstractions.Simulation;

/// <summary>The deterministic, raw cross-module input contract to a full simulation evaluation.</summary>
public sealed record SimulationEvaluationRequestContract(
    SimulationMetadata Metadata,
    SimulationScenario Scenario,
    PredictiveSafetyEvaluationResultContract? PredictiveResult,
    SafetyGraphModel? Graph);

/// <summary>The deterministic output contract of a full simulation evaluation.</summary>
public sealed record SimulationEvaluationResultContract(
    SimulationMetadata Metadata,
    SimulationOutcome Outcome);
