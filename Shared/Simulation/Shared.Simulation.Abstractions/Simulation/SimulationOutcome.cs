// ============================================================================
//  Project:        Shared.Simulation.Abstractions
//  Module:         Simulation
//  File:           SimulationOutcome.cs
//  Created:        2026-08-10
//  Description:    Phase 17 deliverable. The unified, top-level outcome of
//                  running a scenario end-to-end: SimulationReplayEngine.Replay
//                  (deterministic ordering validation) plus
//                  SimulationScenarioEngine.Evaluate (deterministic
//                  cross-module escalation), combined by
//                  SimulationEvaluationPipeline. RequiresOverallEscalation/
//                  OverallPriority are worst-case-wins across both --
//                  an invalid replay never gets silently ignored just
//                  because the scenario itself didn't escalate.
// ============================================================================
using Shared.PredictiveSafety.Abstractions.Common;
using Shared.Simulation.Abstractions.Replay;
using Shared.Simulation.Abstractions.Scenario;

namespace Shared.Simulation.Abstractions.Simulation;

/// <summary>The unified, top-level outcome of running a scenario end-to-end: replay validation plus scenario evaluation.</summary>
public sealed record SimulationOutcome(
    SimulationReplayResult Replay,
    SimulationScenarioResult Scenario,
    bool RequiresOverallEscalation,
    PredictivePriority OverallPriority);
