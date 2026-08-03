// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement.Routing
//  Layer:          Domain
//  File:           RouteSimulationOutcome.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Deterministic output of RouteSimulationEngine.Simulate over a RouteDefinition's
//                  current ordered steps. Genuinely new domain concept -- no critical-path or
//                  bottleneck-detection logic exists anywhere else in the platform.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ProcessManagement.Routing.ValueObjects;

/// <summary>Deterministic simulation output for a single <c>RouteDefinition</c>'s current step sequence.</summary>
/// <param name="TotalDurationMinutes">Sum of every step's estimated duration.</param>
/// <param name="CriticalPathStepNumbers">
/// Step numbers, in order, that make up the critical path. Because routes are strictly
/// sequential (no branching), the critical path is always the full ordered step sequence.
/// </param>
/// <param name="ResourceConflictCount">
/// Count of distinct resource names required by more than one step in the route -- a
/// deterministic proxy for scheduling contention across a single sequential execution.
/// </param>
/// <param name="MaterialShortageCount">Count of steps classified <c>Blocked</c> (missing instructions/readiness).</param>
/// <param name="BottleneckStepNumbers">
/// Step numbers whose estimated duration exceeds 1.5x the route's average step duration.
/// </param>
public sealed record RouteSimulationOutcome(
    int TotalDurationMinutes,
    IReadOnlyList<int> CriticalPathStepNumbers,
    int ResourceConflictCount,
    int MaterialShortageCount,
    IReadOnlyList<int> BottleneckStepNumbers);
