// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.PredictiveScheduling
//  Layer:          Domain
//  File:           IConstraintRule.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Phase 5 Predictive Scheduling Engine)
//  SchemaVersion:  2026.1
//  Description:    Contract for a single, composable rule in the predictive scheduling
//                  constraint pipeline evaluated by ConstraintEvaluator. Implementations
//                  MUST be pure/deterministic and MUST NOT perform infrastructure I/O.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.PredictiveScheduling.Services;

/// <summary>A single, composable rule evaluated by <see cref="ConstraintEvaluator"/>.</summary>
public interface IConstraintRule
{
    /// <summary>Stable machine-readable name of this rule, used as <see cref="ConstraintViolation.RuleName"/>.</summary>
    string RuleName { get; }

    /// <summary>
    /// Evaluates the candidate <paramref name="assignment"/> for <paramref name="task"/> against the
    /// currently known availability windows and already-committed assignments, returning zero or more
    /// violations. MUST be deterministic and side-effect free.
    /// </summary>
    IReadOnlyList<ConstraintViolation> Evaluate(
        SchedulableTask task,
        ScheduleAssignment assignment,
        IReadOnlyList<ResourceAvailabilityWindow> availability,
        IReadOnlyList<ScheduleAssignment> committedAssignments);
}
