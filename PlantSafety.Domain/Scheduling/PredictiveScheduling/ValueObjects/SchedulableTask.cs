// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.PredictiveScheduling
//  Layer:          Domain
//  File:           SchedulableTask.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Phase 5 Predictive Scheduling Engine)
//  SchemaVersion:  2026.1
//  Description:    Immutable value object representing a unit of work the predictive
//                  scheduling rule engine must place onto a resource and time window.
//                  Sourced from PMScheduling, WorkOrderPriority, HazardIsolationSequencing,
//                  EmergencyDrillPlanning and Operational.PredictiveMaintenance aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.PredictiveScheduling.ValueObjects;

/// <summary>An immutable unit of schedulable work fed into the predictive scheduling rule engine.</summary>
public sealed record SchedulableTask
{
    /// <summary>Identity of this schedulable task (mirrors the source aggregate identifier).</summary>
    public SchedulableTaskId TaskId { get; }

    /// <summary>Free-text label describing the source work item (e.g. "PM-4821 Conveyor B Inspection").</summary>
    public string Description { get; }

    /// <summary>Estimated duration required to complete the task.</summary>
    public TimeSpan EstimatedDuration { get; }

    /// <summary>Earliest UTC instant at which the task may begin.</summary>
    public UtcTimestamp EarliestStartUtc { get; }

    /// <summary>Latest UTC instant by which the task must finish.</summary>
    public UtcTimestamp LatestFinishUtc { get; }

    /// <summary>Base priority weight before dynamic adjustment (higher schedules first).</summary>
    public double BasePriorityWeight { get; }

    /// <summary>Number of crew members required to execute the task concurrently.</summary>
    public int RequiredCrewSize { get; init; } = 1;

    /// <summary>Identifier of the machine the task must be performed on, if machine-bound.</summary>
    public SchedulingResourceId? RequiredMachineId { get; init; }

    /// <summary>Identifier of the crew/team the task must be performed by, if crew-bound.</summary>
    public SchedulingResourceId? RequiredCrewId { get; init; }

    /// <summary>True when the task involves hazard isolation, lockout/tagout, or another safety-critical procedure.</summary>
    public bool IsSafetyCritical { get; init; }

    /// <summary>True when the source work item is already flagged overdue.</summary>
    public bool IsOverdue { get; init; }

    /// <summary>Number of times this task has previously slipped/been rescheduled.</summary>
    public int DeferralCount { get; init; }

    /// <summary>Creates a validated SchedulableTask.</summary>
    public SchedulableTask(
        SchedulableTaskId taskId,
        string description,
        TimeSpan estimatedDuration,
        UtcTimestamp earliestStartUtc,
        UtcTimestamp latestFinishUtc,
        double basePriorityWeight)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("SchedulableTask.Description must not be empty.", nameof(description));
        if (estimatedDuration <= TimeSpan.Zero)
            throw new ArgumentException("SchedulableTask.EstimatedDuration must be positive.", nameof(estimatedDuration));
        if (latestFinishUtc.Value <= earliestStartUtc.Value)
            throw new ArgumentException("SchedulableTask.LatestFinishUtc must be after EarliestStartUtc.", nameof(latestFinishUtc));

        TaskId = taskId;
        Description = description;
        EstimatedDuration = estimatedDuration;
        EarliestStartUtc = earliestStartUtc;
        LatestFinishUtc = latestFinishUtc;
        BasePriorityWeight = basePriorityWeight;
    }

    /// <summary>Total available slack between the earliest start and the latest finish, net of estimated duration.</summary>
    public TimeSpan Slack => (LatestFinishUtc.Value - EarliestStartUtc.Value) - EstimatedDuration;
}
