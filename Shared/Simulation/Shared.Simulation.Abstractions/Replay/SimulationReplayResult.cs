// ============================================================================
//  Project:        Shared.Simulation.Abstractions
//  Module:         Replay
//  File:           SimulationReplayResult.cs
//  Created:        2026-08-10
//  Description:    Phase 17 deliverable. The deterministic output of a
//                  single scenario replay.
// ============================================================================
namespace Shared.Simulation.Abstractions.Replay;

/// <summary>The deterministic output of a single scenario replay.</summary>
public sealed record SimulationReplayResult(
    string ScenarioId,
    IReadOnlyList<int> ReplayedStepIndices,
    bool IsValidReplay,
    IReadOnlyList<string> AppliedRuleCodes);
