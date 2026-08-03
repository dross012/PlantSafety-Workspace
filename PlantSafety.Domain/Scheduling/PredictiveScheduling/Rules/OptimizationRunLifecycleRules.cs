// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.PredictiveScheduling
//  Layer:          Domain
//  File:           OptimizationRunLifecycleRules.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Defines the explicit, permitted lifecycle transitions for the OptimizationRun
//                  state machine (Pending -> Running -> Completed|Failed). Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Scheduling.PredictiveScheduling.Enums;

namespace PlantSafety.Domain.Scheduling.PredictiveScheduling.Rules;

/// <summary>Defines permitted lifecycle transitions for the OptimizationRun state machine.</summary>
public static class OptimizationRunLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the run may begin executing (Pending -&gt; Running).</summary>
    public static bool CanBegin(OptimizationRunStatus current) => current == OptimizationRunStatus.Pending;

    /// <summary>Returns <see langword="true"/> when the run may be marked Completed (Running -&gt; Completed).</summary>
    public static bool CanComplete(OptimizationRunStatus current) => current == OptimizationRunStatus.Running;

    /// <summary>Returns <see langword="true"/> when the run may be marked Failed (Pending|Running -&gt; Failed).</summary>
    public static bool CanFail(OptimizationRunStatus current)
        => current is OptimizationRunStatus.Pending or OptimizationRunStatus.Running;

    /// <summary>Returns <see langword="true"/> when <paramref name="status"/> is a terminal (Completed/Failed) status.</summary>
    public static bool IsTerminal(OptimizationRunStatus status)
        => status is OptimizationRunStatus.Completed or OptimizationRunStatus.Failed;
}
