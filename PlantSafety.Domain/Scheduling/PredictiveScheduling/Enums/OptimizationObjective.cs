// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.PredictiveScheduling
//  Layer:          Domain
//  File:           OptimizationObjective.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Phase 5 Predictive Scheduling Engine)
//  SchemaVersion:  2026.1
//  Description:    Enumerates the objectives the scheduling optimization loop can be
//                  configured to pursue. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.PredictiveScheduling.Enums;

/// <summary>Objective that an optimization strategy is being scored against.</summary>
public enum OptimizationObjective
{
    /// <summary>Minimize total elapsed time to complete all scheduled tasks (makespan).</summary>
    MinimizeMakespan = 0,

    /// <summary>Maximize the aggregate safety margin (favor safety-critical tasks and rest compliance).</summary>
    MaximizeSafetyMargin = 1,

    /// <summary>Minimize the number of unscheduled/deferred tasks.</summary>
    MaximizeCoverage = 2,

    /// <summary>Balance resource workload evenly across crews and machines.</summary>
    BalanceWorkload = 3,
}
