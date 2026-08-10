// ============================================================================
//  Project:        Shared.Simulation.Abstractions
//  Module:         Simulation
//  File:           SimulationScenario.cs
//  Created:        2026-08-10
//  Description:    Phase 17 deliverable. The immutable, versioned scenario
//                  aggregate: a deterministic, caller-supplied ScenarioId
//                  (never a random Guid, so the same scenario definition
//                  always identifies itself the same way across replays)
//                  plus an ordered list of SimulationSteps. Cross-module
//                  scenario composition (Hazard + Maintenance + Routing +
//                  Scheduling + Workflow + PredictiveInference in one
//                  scenario) is supported simply by Steps containing more
//                  than one SimulationStepType -- no special-case type is
//                  needed for "composed" scenarios.
// ============================================================================
namespace Shared.Simulation.Abstractions.Simulation;

/// <summary>The immutable, versioned scenario aggregate: a deterministic identity plus an ordered list of steps.</summary>
public sealed record SimulationScenario(
    string ScenarioId,
    string Name,
    IReadOnlyList<SimulationStep> Steps);
