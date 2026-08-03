// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.PredictiveScheduling
//  Layer:          Domain
//  File:           ShiftCrewRuleSet.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Phase 5 Predictive Scheduling Engine)
//  SchemaVersion:  2026.1
//  Description:    Constraint rule enforcing shift/crew scheduling policy: sufficient crew
//                  headcount, no crew double-booking, and a maximum-consecutive-hours-on-shift
//                  guard (mirrors the intent of Scheduling.ShiftFatigue). Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.PredictiveScheduling.Services;

/// <summary>Enforces crew headcount, double-booking, and maximum-consecutive-hours shift rules.</summary>
public sealed class ShiftCrewRuleSet : IConstraintRule
{
    private readonly TimeSpan _maxConsecutiveShiftDuration;

    /// <summary>Creates the rule set with the platform default 12-hour maximum consecutive shift.</summary>
    public ShiftCrewRuleSet() : this(TimeSpan.FromHours(12)) { }

    /// <summary>Creates the rule set with an explicit maximum consecutive shift duration.</summary>
    public ShiftCrewRuleSet(TimeSpan maxConsecutiveShiftDuration)
    {
        if (maxConsecutiveShiftDuration <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(maxConsecutiveShiftDuration));
        _maxConsecutiveShiftDuration = maxConsecutiveShiftDuration;
    }

    /// <inheritdoc/>
    public string RuleName => "ShiftCrewRules";

    /// <inheritdoc/>
    public IReadOnlyList<ConstraintViolation> Evaluate(
        SchedulableTask task,
        ScheduleAssignment assignment,
        IReadOnlyList<ResourceAvailabilityWindow> availability,
        IReadOnlyList<ScheduleAssignment> committedAssignments)
    {
        var violations = new List<ConstraintViolation>();

        if (task.RequiredCrewId is not { } requiredCrewId)
            return violations;

        if (assignment.ResourceId != requiredCrewId)
        {
            violations.Add(ConstraintViolation.Blocking(RuleName, task.TaskId,
                $"Task requires crew {requiredCrewId} but assignment targets resource {assignment.ResourceId}."));
            return violations;
        }

        var crewWindow = availability.FirstOrDefault(w => w.ResourceId == requiredCrewId);
        if (crewWindow is null)
        {
            violations.Add(ConstraintViolation.Blocking(RuleName, task.TaskId,
                $"No availability window declared for crew {requiredCrewId}."));
            return violations;
        }

        if (crewWindow.Capacity < task.RequiredCrewSize)
        {
            violations.Add(ConstraintViolation.Blocking(RuleName, task.TaskId,
                $"Crew {requiredCrewId} capacity ({crewWindow.Capacity}) is below the required headcount ({task.RequiredCrewSize})."));
        }

        var crewAssignmentsToday = committedAssignments
            .Where(a => a.ResourceId == requiredCrewId && a.OverlapsWith(assignment))
            .ToList();

        if (crewAssignmentsToday.Count > 0)
        {
            violations.Add(ConstraintViolation.Blocking(RuleName, task.TaskId,
                $"Crew {requiredCrewId} is already committed to {crewAssignmentsToday.Count} overlapping task(s)."));
        }

        var sameShiftAssignments = committedAssignments
            .Where(a => a.ResourceId == requiredCrewId)
            .Append(assignment)
            .OrderBy(a => a.StartUtc.Value)
            .ToList();

        if (sameShiftAssignments.Count > 0)
        {
            var shiftStart = sameShiftAssignments.Min(a => a.StartUtc.Value);
            var shiftEnd = sameShiftAssignments.Max(a => a.EndUtc.Value);
            var span = shiftEnd - shiftStart;

            if (span > _maxConsecutiveShiftDuration)
            {
                violations.Add(ConstraintViolation.Warn(RuleName, task.TaskId,
                    $"Crew {requiredCrewId} consecutive shift span ({span.TotalHours:F1}h) exceeds the " +
                    $"{_maxConsecutiveShiftDuration.TotalHours:F1}h fatigue guard."));
            }
        }

        return violations;
    }
}
