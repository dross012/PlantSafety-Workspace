// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.PredictiveScheduling
//  Layer:          Domain
//  File:           MachineAvailabilityRuleSet.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Phase 5 Predictive Scheduling Engine)
//  SchemaVersion:  2026.1
//  Description:    Constraint rule enforcing machine-bound scheduling policy: the machine
//                  must be free (per its declared availability window) and must not be
//                  double-booked by an overlapping committed assignment. Additive-only
//                  evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.PredictiveScheduling.Services;

/// <summary>Enforces machine availability-window and no-double-booking scheduling rules.</summary>
public sealed class MachineAvailabilityRuleSet : IConstraintRule
{
    /// <inheritdoc/>
    public string RuleName => "MachineAvailability";

    /// <inheritdoc/>
    public IReadOnlyList<ConstraintViolation> Evaluate(
        SchedulableTask task,
        ScheduleAssignment assignment,
        IReadOnlyList<ResourceAvailabilityWindow> availability,
        IReadOnlyList<ScheduleAssignment> committedAssignments)
    {
        var violations = new List<ConstraintViolation>();

        if (task.RequiredMachineId is not { } requiredMachineId)
            return violations;

        if (assignment.ResourceId != requiredMachineId)
        {
            violations.Add(ConstraintViolation.Blocking(RuleName, task.TaskId,
                $"Task requires machine {requiredMachineId} but assignment targets resource {assignment.ResourceId}."));
            return violations;
        }

        var machineWindow = availability.FirstOrDefault(w => w.ResourceId == requiredMachineId);
        if (machineWindow is null || !machineWindow.Contains(assignment.StartUtc, assignment.EndUtc))
        {
            violations.Add(ConstraintViolation.Blocking(RuleName, task.TaskId,
                $"Machine {requiredMachineId} has no declared availability window covering " +
                $"{assignment.StartUtc.Value:O}-{assignment.EndUtc.Value:O}."));
        }

        var doubleBooked = committedAssignments.Any(a =>
            a.ResourceId == requiredMachineId && a.OverlapsWith(assignment));

        if (doubleBooked)
        {
            violations.Add(ConstraintViolation.Blocking(RuleName, task.TaskId,
                $"Machine {requiredMachineId} is already booked for an overlapping task."));
        }

        return violations;
    }
}
