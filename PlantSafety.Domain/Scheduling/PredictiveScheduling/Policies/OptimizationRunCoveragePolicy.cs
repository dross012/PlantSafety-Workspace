// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.PredictiveScheduling
//  Layer:          Domain
//  File:           OptimizationRunCoveragePolicy.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain policy classifying a completed OptimizationRun's coverage outcome, so
//                  callers (orchestration/UI) can decide whether a run's result requires manual
//                  review (e.g. safety-critical tasks left unscheduled) rather than being applied
//                  automatically. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.PredictiveScheduling.Policies;

/// <summary>Coverage outcome classification for a completed OptimizationRun.</summary>
public enum OptimizationRunCoverageOutcome
{
    /// <summary>Every input task was placed onto a feasible schedule assignment.</summary>
    FullCoverage = 0,

    /// <summary>Some tasks were left unscheduled, but none were safety-critical.</summary>
    PartialCoverage = 1,

    /// <summary>Some tasks were left unscheduled and at least one is safety-critical -- requires manual review before acting on the result.</summary>
    RequiresManualReview = 2,
}

/// <summary>Classifies a completed OptimizationRun's coverage outcome.</summary>
public static class OptimizationRunCoveragePolicy
{
    /// <summary>
    /// Classifies the coverage outcome for a run given the number of unscheduled tasks and
    /// whether any of them are safety-critical.
    /// </summary>
    public static OptimizationRunCoverageOutcome Classify(int unscheduledTaskCount, bool anyUnscheduledIsSafetyCritical)
    {
        if (unscheduledTaskCount == 0)
            return OptimizationRunCoverageOutcome.FullCoverage;

        return anyUnscheduledIsSafetyCritical
            ? OptimizationRunCoverageOutcome.RequiresManualReview
            : OptimizationRunCoverageOutcome.PartialCoverage;
    }
}
