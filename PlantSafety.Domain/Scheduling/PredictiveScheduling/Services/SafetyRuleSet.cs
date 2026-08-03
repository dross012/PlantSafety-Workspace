// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.PredictiveScheduling
//  Layer:          Domain
//  File:           SafetyRuleSet.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Phase 5 Predictive Scheduling Engine)
//  SchemaVersion:  2026.1
//  Description:    Constraint rule enforcing safety policy for safety-critical tasks
//                  (hazard isolation, lockout/tagout style work): such tasks may not be
//                  scheduled concurrently with another safety-critical task on the same
//                  machine, and require a minimum crew size floor. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.PredictiveScheduling.Services;

/// <summary>Enforces safety-critical task isolation and minimum-crew scheduling rules.</summary>
public sealed class SafetyRuleSet : IConstraintRule
{
    private const int MinimumSafetyCriticalCrewSize = 2;

    /// <inheritdoc/>
    public string RuleName => "SafetyRules";

    /// <inheritdoc/>
    public IReadOnlyList<ConstraintViolation> Evaluate(
        SchedulableTask task,
        ScheduleAssignment assignment,
        IReadOnlyList<ResourceAvailabilityWindow> availability,
        IReadOnlyList<ScheduleAssignment> committedAssignments)
    {
        var violations = new List<ConstraintViolation>();

        if (!task.IsSafetyCritical)
            return violations;

        if (task.RequiredCrewSize < MinimumSafetyCriticalCrewSize)
        {
            violations.Add(ConstraintViolation.Blocking(RuleName, task.TaskId,
                $"Safety-critical tasks require a minimum crew of {MinimumSafetyCriticalCrewSize} " +
                $"(requested {task.RequiredCrewSize})."));
        }

        // A safety-critical task must not overlap another safety-critical task on the same
        // machine/resource -- isolation and lockout/tagout work must never run concurrently
        // with other safety-critical procedures against the same equipment.
        var conflictingSafetyWork = committedAssignments.Any(a =>
            a.ResourceId == assignment.ResourceId && a.OverlapsWith(assignment));

        if (conflictingSafetyWork)
        {
            violations.Add(ConstraintViolation.Blocking(RuleName, task.TaskId,
                "A safety-critical task cannot be scheduled concurrently with another task on the same resource."));
        }

        return violations;
    }
}
