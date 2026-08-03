// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.PredictiveScheduling
//  Layer:          Domain
//  File:           OptimizationRunStatus.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Enumerates the lifecycle states of an OptimizationRun aggregate -- one
//                  invocation of the predictive scheduling optimization pipeline
//                  (MultiObjectiveScheduleOptimizer over the configured IOptimizationStrategy
//                  set). Mirrors the plain-enum style already established by this module's
//                  OptimizationObjective/ConstraintSeverity enums. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.PredictiveScheduling.Enums;

/// <summary>Lifecycle state of a single OptimizationRun.</summary>
public enum OptimizationRunStatus
{
    /// <summary>Run has been requested and its inputs captured, but the optimization pipeline has not yet started.</summary>
    Pending = 0,

    /// <summary>The optimization pipeline (strategy set + MultiObjectiveScheduleOptimizer selection) is currently executing.</summary>
    Running = 1,

    /// <summary>The optimization pipeline completed and produced a result (with or without tolerated violations).</summary>
    Completed = 2,

    /// <summary>The optimization pipeline could not produce a result (e.g. an unhandled exception during strategy execution).</summary>
    Failed = 3,
}
