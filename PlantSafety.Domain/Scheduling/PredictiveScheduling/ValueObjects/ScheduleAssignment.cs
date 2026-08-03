// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.PredictiveScheduling
//  Layer:          Domain
//  File:           ScheduleAssignment.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Phase 5 Predictive Scheduling Engine)
//  SchemaVersion:  2026.1
//  Description:    Immutable value object representing a candidate or committed placement
//                  of a SchedulableTask onto a resource within a time window. Produced by
//                  optimization strategies and validated by the ConstraintEvaluator.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.PredictiveScheduling.ValueObjects;

/// <summary>A candidate or committed placement of a task onto a resource and time window.</summary>
public sealed record ScheduleAssignment
{
    /// <summary>Identity of the task being placed.</summary>
    public SchedulableTaskId TaskId { get; }

    /// <summary>Identity of the resource the task is placed against.</summary>
    public SchedulingResourceId ResourceId { get; }

    /// <summary>UTC instant the task is planned to start.</summary>
    public UtcTimestamp StartUtc { get; }

    /// <summary>UTC instant the task is planned to finish.</summary>
    public UtcTimestamp EndUtc { get; }

    /// <summary>Composite priority weight used to order this assignment relative to others.</summary>
    public double EffectivePriorityWeight { get; init; }

    /// <summary>Creates a validated ScheduleAssignment.</summary>
    public ScheduleAssignment(SchedulableTaskId taskId, SchedulingResourceId resourceId, UtcTimestamp startUtc, UtcTimestamp endUtc)
    {
        if (endUtc.Value <= startUtc.Value)
            throw new ArgumentException("ScheduleAssignment.EndUtc must be after StartUtc.", nameof(endUtc));

        TaskId = taskId;
        ResourceId = resourceId;
        StartUtc = startUtc;
        EndUtc = endUtc;
    }

    /// <summary>True when this assignment's [Start, End) interval overlaps another assignment's interval.</summary>
    public bool OverlapsWith(ScheduleAssignment other) =>
        StartUtc.Value < other.EndUtc.Value && other.StartUtc.Value < EndUtc.Value;
}
